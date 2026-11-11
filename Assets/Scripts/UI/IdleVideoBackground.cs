using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Exhibition.UI
{
    /// <summary>
    /// 타이틀 배경 영상(c02)을 소리 없이, 이음매 없이 반복한다. 정지 배경 Intro_bg 바로 위, 로고와 다른 타이틀 요소 아래에 있다.
    /// VideoPlayer의 isLooping은 실행 중인 디코더를 0번 프레임으로 되감아 마지막 프레임이 약 50ms 멈추고, 가끔 8프레임 전
    /// 화면이 한 번 보인다. 그래서 플레이어 2개를 번갈아 쓴다. 하나가 재생하는 동안 다른 하나는 처음부터 다시 재생해 0번 프레임에서
    /// 멈춰 두고, 마지막 프레임이 한 프레임 길이만큼 보이면 화면을 넘긴다. 첫 프레임이 준비되기 전이나 재생 실패 시에는
    /// 영상 이미지를 숨겨 아래 정지 배경이 보인다.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RawImage), typeof(VideoPlayer))]
    public sealed class IdleVideoBackground : MonoBehaviour
    {
        [SerializeField] private VideoClip clip;

        private enum SlotState { Idle, WaitingForFirstFrame, Pausing, Checking, Ready, Playing }

        /// <summary>디코더 하나와 그 렌더 대상, 첫 프레임 도착을 확인할 1x1 텍스처.</summary>
        private sealed class Slot
        {
            public VideoPlayer Player;
            public RenderTexture Target;
            public RenderTexture Check;
            public SlotState State;
            /// <summary>frameReady가 첫 프레임을 알린 Time.frameCount.</summary>
            public int FirstFrameEventAt;
            /// <summary>다시 준비할 때마다 늘려, 이전 시도의 늦은 확인 결과를 무시한다.</summary>
            public int Attempt;
            /// <summary>확인 요청을 보낼 때의 Attempt.</summary>
            public int CheckAttempt;
            /// <summary>확인 콜백. 준비마다 람다를 새로 만들지 않도록 한 번만 만든다.</summary>
            public Action<AsyncGPUReadbackRequest> OnChecked;
        }

        private readonly Slot[] _slots = { new Slot(), new Slot() };
        private RawImage _image;
        private VideoPlayer _addedPlayer;
        /// <summary>화면에 보이는 슬롯 번호. 첫 프레임 전에는 -1.</summary>
        private int _active = -1;
        /// <summary>현재 슬롯의 마지막 프레임을 처음 본 실제 시간(초). 아직 못 봤으면 -1.</summary>
        private double _lastFrameSeenAt = -1;
        private long _lastFrame;
        /// <summary>영상 한 프레임 길이(초).</summary>
        private double _frameSeconds;
        /// <summary>true면 재생을 포기하고 정지 배경만 보인다.</summary>
        private bool _failed;
        /// <summary>true면 GPU 읽기로 첫 프레임이 실제로 그려졌는지 확인할 수 있다.</summary>
        private bool _canCheckFrames;
        /// <summary>화면을 넘긴 렌더 프레임에서만 쓰는 재생 속도(배). 영상 시계를 렌더 프레임 반 개만큼 앞당긴다.</summary>
        private const float PhaseShiftSpeed = 1.5f;
        /// <summary>재생 속도를 1배로 되돌릴 기준 Time.frameCount. 보정 중이 아니면 -1.</summary>
        private int _phaseShiftFrame = -1;

        /// <summary>영상 이미지가 켜져 있으면 true(정지 배경 위에 영상이 보임).</summary>
        public bool IsShowing => _image && _image.enabled;

        /// <summary>지금 화면에 보이는 플레이어. 첫 프레임 전에는 첫 번째 플레이어.</summary>
        public VideoPlayer Player => _slots[_active >= 0 ? _active : 0].Player;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _image.raycastTarget = false;
            _slots[0].Player = GetComponent<VideoPlayer>();
            _addedPlayer = gameObject.AddComponent<VideoPlayer>();
            _slots[1].Player = _addedPlayer;
            foreach (var slot in _slots)
            {
                Configure(slot.Player);
                var owner = slot;
                owner.OnChecked = request => OnChecked(owner, request);
            }
            _canCheckFrames = SystemInfo.supportsAsyncGPUReadback;
        }

        private static void Configure(VideoPlayer player)
        {
            player.playOnAwake = false;
            // 씬에 저장된 isLooping 값과 관계없이 끈다. 반복은 두 플레이어 교대로 처리한다.
            player.isLooping = false;
            player.waitForFirstFrame = true;
            player.skipOnDrop = true;
            player.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.source = VideoSource.VideoClip;
            player.audioOutputMode = VideoAudioOutputMode.None;
        }

        private void OnEnable()
        {
            HideVideo();
            _active = -1;
            _lastFrameSeenAt = -1;
            _failed = false;
            if (!clip)
            {
                Debug.LogError("[IdleVideoBackground] Video clip is not assigned. Showing the still background.", this);
                _failed = true;
                return;
            }
            _lastFrame = (long)clip.frameCount - 1;
            _frameSeconds = clip.frameRate > 0 ? 1.0 / clip.frameRate : 1.0 / 30.0;
            for (int i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                slot.Target = new RenderTexture((int)clip.width, (int)clip.height, 0, RenderTextureFormat.ARGB32)
                {
                    name = "IdleVideoBackground" + i,
                    useMipMap = false,
                    autoGenerateMips = false,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                slot.Target.Create();
                slot.Check = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32) { name = "IdleVideoBackgroundCheck" + i };
                slot.Check.Create();
                slot.Player.clip = clip;
                slot.Player.targetTexture = slot.Target;
                slot.Player.frameReady += OnFrameReady;
                slot.Player.errorReceived += OnError;
                Preroll(slot);
            }
        }

        /// <summary>슬롯을 새 디코더로 처음부터 재생한다. 0번 프레임이 렌더 대상에 들어오면 멈춘다.</summary>
        private static void Preroll(Slot slot)
        {
            ++slot.Attempt;
            slot.State = SlotState.WaitingForFirstFrame;
            slot.Player.Stop();
            slot.Player.playbackSpeed = 1f;
            // 투명하게 비워 두어, 아래 확인에서 프레임이 들어왔는지(불투명) 빠졌는지(투명) 구분한다.
            var previous = RenderTexture.active;
            RenderTexture.active = slot.Target;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
            slot.Player.sendFrameReadyEvents = true;
            slot.Player.Play();
        }

        private void OnFrameReady(VideoPlayer source, long frame)
        {
            var slot = Find(source);
            if (slot == null || slot.State != SlotState.WaitingForFirstFrame) return;
            source.sendFrameReadyEvents = false;
            slot.FirstFrameEventAt = Time.frameCount;
            slot.State = SlotState.Pausing;
        }

        /// <summary>
        /// frameReady는 프레임이 렌더 대상에 복사되기 전에 올 수 있어 한 렌더 프레임 뒤에 멈춘다. 다시 시작한 플레이어가
        /// 0번 프레임을 아예 그리지 않는 경우가 가끔 있으므로 1x1 GPU 읽기로 불투명한지 확인하고, 비어 있으면 다시 준비한다.
        /// </summary>
        private void AdvancePreroll(Slot slot)
        {
            if (slot.State != SlotState.Pausing || Time.frameCount <= slot.FirstFrameEventAt) return;
            if (slot.Player.frame > 0)
            {
                // 끊김 때문에 0번 프레임을 지나쳤다.
                Preroll(slot);
                return;
            }
            slot.Player.Pause();
            if (!_canCheckFrames)
            {
                slot.State = SlotState.Ready;
                return;
            }
            slot.State = SlotState.Checking;
            slot.CheckAttempt = slot.Attempt;
            Graphics.Blit(slot.Target, slot.Check);
            AsyncGPUReadback.Request(slot.Check, 0, TextureFormat.RGBA32, slot.OnChecked);
        }

        private void OnChecked(Slot slot, AsyncGPUReadbackRequest request)
        {
            if (this == null || !isActiveAndEnabled || slot.Attempt != slot.CheckAttempt || slot.State != SlotState.Checking) return;
            if (request.hasError)
            {
                // 확인 자체가 실패하면 반복을 멈추지 않도록 플레이어를 믿는다.
                slot.State = SlotState.Ready;
                return;
            }
            // 알파 250 이상이면 프레임이 그려진 것(영상은 불투명).
            if (request.GetData<byte>()[3] >= 250)
            {
                slot.State = SlotState.Ready;
                return;
            }
            Debug.Log("[IdleVideoBackground] The first frame did not reach the render target; preparing that player again.", this);
            Preroll(slot);
        }

        private void Update()
        {
            if (_failed) return;
            foreach (var slot in _slots) AdvancePreroll(slot);
            if (_active < 0)
            {
                if (_slots[0].State == SlotState.Ready) Show(0);
                return;
            }
            var current = _slots[_active];
            if (_phaseShiftFrame >= 0 && Time.frameCount > _phaseShiftFrame)
            {
                current.Player.playbackSpeed = 1f;
                _phaseShiftFrame = -1;
            }
            if (current.Player.frame < _lastFrame)
            {
                _lastFrameSeenAt = -1;
                return;
            }
            double now = Time.unscaledTimeAsDouble;
            if (_lastFrameSeenAt < 0) _lastFrameSeenAt = now;
            // 마지막 프레임을 한 프레임 길이만큼 보여 준 뒤 넘긴다. 다음 슬롯이 늦으면 앞으로 튀지 않도록 마지막 프레임을 유지한다.
            if (now - _lastFrameSeenAt + 0.001 >= _frameSeconds && _slots[1 - _active].State == SlotState.Ready)
            {
                Show(1 - _active);
                Preroll(current);
            }
        }

        private void Show(int index)
        {
            var slot = _slots[index];
            _active = index;
            _lastFrameSeenAt = -1;
            slot.State = SlotState.Playing;
            _image.texture = slot.Target;
            _image.enabled = true;
            // 렌더 프레임 순간에 시작하면 영상 프레임 경계가 렌더 순간과 겹쳐, 프레임 시간의 작은 흔들림에도 한 프레임이
            // 16ms·50ms로 번갈아 보인다. 첫 렌더 프레임만 1.5배로 진행해 경계를 렌더 프레임 사이(반 프레임 뒤)로 옮긴다.
            slot.Player.playbackSpeed = PhaseShiftSpeed;
            _phaseShiftFrame = Time.frameCount;
            slot.Player.Play();
        }

        private Slot Find(VideoPlayer player)
        {
            foreach (var slot in _slots) if (slot.Player == player) return slot;
            return null;
        }

        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError("[IdleVideoBackground] Background video failed. Showing the still background. " + message, this);
            _failed = true;
            HideVideo();
        }

        private void HideVideo()
        {
            if (!_image) return;
            _image.enabled = false;
            _image.texture = null;
        }

        private void OnDisable()
        {
            HideVideo();
            foreach (var slot in _slots)
            {
                if (slot.Player)
                {
                    slot.Player.frameReady -= OnFrameReady;
                    slot.Player.errorReceived -= OnError;
                    slot.Player.sendFrameReadyEvents = false;
                    slot.Player.Stop();
                    slot.Player.targetTexture = null;
                }
                slot.State = SlotState.Idle;
                ++slot.Attempt;
                Release(ref slot.Target);
                Release(ref slot.Check);
            }
            _active = -1;
            _phaseShiftFrame = -1;
        }

        private static void Release(ref RenderTexture texture)
        {
            if (!texture) return;
            texture.Release();
            Destroy(texture);
            texture = null;
        }

        private void OnDestroy()
        {
            if (_addedPlayer) Destroy(_addedPlayer);
        }
    }
}
