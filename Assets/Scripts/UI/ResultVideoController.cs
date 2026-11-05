using System;
using System.Threading;
using Core;
using Exhibition.Blocks;
using Cysharp.Threading.Tasks;
using HuliacDev.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Exhibition.UI
{
    [DisallowMultipleComponent]
    public sealed class ResultVideoController : MonoBehaviour
    {
        public enum ScreenState { Advertisement, Preparing, Playing, Finished, Failed }
        [SerializeField] private GameObject advertisement, videoPanel, endPopup;
        [SerializeField] private GameObject playbackHint;
        [Tooltip("Advertisement button hint. Stays hidden until the button actually works.")]
        [SerializeField] private UIAnimatedElement advertisementHint;
        [SerializeField] private RawImage videoImage;
        [SerializeField] private VideoPlayer player;
        [SerializeField] private LocalizedImageSet localization;
        [SerializeField] private AdProductHighlight productHighlight;
        private GameManager _manager;
        private RfidReaderHub _readers;
        private InactivityTimer _timer;
        private CancellationTokenSource _screenCts, _playbackCts;
        private RenderTexture _target;
        private VideoClip _clip;
        private UIPopupPresentation _popupMotion;
        private UIAnimatedElement _advertisementMotion, _videoMotion;
        private bool _ended;
        private bool _awaitingFirstFrame, _firstFrameReady;
        private long _firstFrame = -1;
        private bool[] _audioMutes;
        private string _videoError;
        private int _remainingBlocks = 15;
        private bool _advertisementHintRequested;
        public ScreenState State { get; private set; }

        private void OnEnable()
        {
            _screenCts = new CancellationTokenSource();
            _manager = FindFirstObjectByType<GameManager>();
            _readers = FindFirstObjectByType<RfidReaderHub>();
            _timer = FindFirstObjectByType<InactivityTimer>();
            if (_manager) _manager.ShortButtonPressed += Continue;
            player.playOnAwake = false;
            player.isLooping = false;
            player.waitForFirstFrame = true;
            player.sendFrameReadyEvents = false;
            player.loopPointReached += OnVideoEnded;
            player.errorReceived += OnVideoError;
            player.frameReady += OnFrameReady;
            _popupMotion = GetComponent<UIPopupPresentation>();
            _advertisementMotion = advertisement.GetComponent<UIAnimatedElement>();
            _videoMotion = videoPanel.GetComponent<UIAnimatedElement>();
            State = ScreenState.Advertisement;
            _remainingBlocks = 15;
            advertisement.SetActive(true);
            _advertisementMotion?.SnapVisible();
            videoImage.enabled = false;
            if (playbackHint) playbackHint.SetActive(false);
            videoPanel.SetActive(false);
            if (_popupMotion) _popupMotion.ResetHidden();
            else endPopup.SetActive(false);
            // Inactive here also keeps the scene entrance from revealing it with the other cards.
            _advertisementHintRequested = false;
            if (advertisementHint)
            {
                advertisementHint.PrepareHidden();
                advertisementHint.gameObject.SetActive(false);
            }
        }

        // No input lock here: the hint appears because input just became available.
        private void LateUpdate()
        {
            if (!advertisementHint) return;
            bool available = (State == ScreenState.Advertisement || State == ScreenState.Failed) && _manager &&
                !_manager.IsTransitioning && !_manager.IsPresentationLocked && (!productHighlight || !productHighlight.IsPlaying);
            if (available == _advertisementHintRequested) return;
            _advertisementHintRequested = available;
            advertisementHint.SetVisibleAsync(available, _screenCts.Token).Forget();
        }

        private void Continue()
        {
            if (productHighlight && productHighlight.IsPlaying) return;
            if (_manager && _manager.IsPresentationLocked) return;
            if (State == ScreenState.Advertisement || State == ScreenState.Failed) PlayAsync().Forget();
            else if (State == ScreenState.Finished) _manager.ReturnToIdleManually();
        }

        private async UniTask PlayAsync()
        {
            if (_playbackCts != null || !_manager || _manager.IsTransitioning) return;
            _manager.Audio.PlayConfirm();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_screenCts.Token, _manager.SessionCancellationToken);
            _playbackCts = source;
            var token = source.Token;
            State = ScreenState.Preparing;
            _timer?.Pause();
            _ended = false;
            _videoError = null;
            _firstFrame = -1;
            _firstFrameReady = _awaitingFirstFrame = false;
            bool originalSkipOnDrop = player.skipOnDrop;
            IDisposable visualLock = _manager.Presentation.LockInput();
            try
            {
                if (_popupMotion) await _popupMotion.SetVisibleAsync(false, token);
                else endPopup.SetActive(false);
                await _manager.Presentation.CoverAsync(token);
                productHighlight?.StopEffects();
                // Keep the complete advertisement covering the camera throughout loading/decoding.
                _advertisementMotion?.SnapVisible();
                advertisement.SetActive(true);
                _videoMotion?.PrepareHidden();
                videoImage.enabled = false;
                videoPanel.SetActive(false);
                await UniTask.WaitUntil(() => localization.IsReady, cancellationToken: token)
                    .Timeout(TimeSpan.FromSeconds(12), DelayType.UnscaledDeltaTime, taskCancellationTokenSource: source);
                var request = Resources.LoadAsync<VideoClip>(localization.VideoResource);
                await request.ToUniTask(cancellationToken: token);
                _clip = request.asset as VideoClip;
                if (!_clip) throw new InvalidOperationException("Video clip is missing: " + localization.VideoResource);
                player.source = VideoSource.VideoClip;
                player.clip = _clip;
                player.renderMode = VideoRenderMode.RenderTexture;
                player.audioOutputMode = VideoAudioOutputMode.Direct;
                _target = new RenderTexture((int)_clip.width, (int)_clip.height, 0, RenderTextureFormat.ARGB32);
                _target.name = "ExhibitionVideo";
                _target.Create();
                player.targetTexture = _target;
                videoImage.texture = _target;
                player.Prepare();
                await UniTask.WaitUntil(() => player.isPrepared || _videoError != null, cancellationToken: token)
                    .Timeout(TimeSpan.FromSeconds(15), DelayType.UnscaledDeltaTime, taskCancellationTokenSource: source);
                ThrowVideoError();

                _audioMutes = new bool[player.audioTrackCount];
                for (ushort track = 0; track < _audioMutes.Length; track++)
                {
                    _audioMutes[track] = player.GetDirectAudioMute(track);
                    player.SetDirectAudioMute(track, true);
                }
                player.skipOnDrop = false;
                player.sendFrameReadyEvents = true;
                _awaitingFirstFrame = true;
                player.Play();
                await UniTask.WaitUntil(() => _firstFrameReady || _videoError != null, cancellationToken: token)
                    .Timeout(TimeSpan.FromSeconds(15), DelayType.UnscaledDeltaTime, taskCancellationTokenSource: source);
                ThrowVideoError();
                if (_firstFrame != 0) throw new InvalidOperationException("Video decoder did not deliver opening frame zero.");
                // The callback pauses frame zero behind the ad. Let its render-target write finish first.
                await UniTask.WaitForEndOfFrame(this, token);
                token.ThrowIfCancellationRequested();
                videoImage.color = Color.white;
                videoImage.enabled = true;
                if (playbackHint) playbackHint.SetActive(true);
                if (_videoMotion) _videoMotion.SnapVisible();
                else videoPanel.SetActive(true);
                _advertisementMotion?.PrepareHidden();
                advertisement.SetActive(false);
                // Present frame zero for one rendered frame, then continue immediately with audio.
                await UniTask.WaitForEndOfFrame(this, token);
                token.ThrowIfCancellationRequested();
                player.Play();
                RestoreAudio();
                player.skipOnDrop = originalSkipOnDrop;
                await _manager.Presentation.UncoverAsync(token);
                State = ScreenState.Playing;
                visualLock.Dispose(); visualLock = null;
                Debug.Log("[ResultVideo] Playing " + localization.VideoResource + ".", this);
                await UniTask.WaitUntil(() => _ended || _videoError != null, cancellationToken: token)
                    .Timeout(TimeSpan.FromSeconds(Math.Max(30, _clip.length + 20)), DelayType.UnscaledDeltaTime, taskCancellationTokenSource: source);
                ThrowVideoError();
                State = ScreenState.Finished;
                if (playbackHint) playbackHint.SetActive(false);
                if (_popupMotion) await _popupMotion.SetVisibleAsync(true, _screenCts.Token);
                else endPopup.SetActive(true);
                Debug.Log("[ResultVideo] Playback completed.", this);
                if (_remainingBlocks == 0) _manager.ReturnToIdleManually();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (this && isActiveAndEnabled && (ex is TimeoutException || !token.IsCancellationRequested))
                {
                    State = ScreenState.Failed;
                    RestoreAdvertisement();
                    // The popup is a child of this panel; its RawImage stays disabled over the ad.
                    _videoMotion?.SnapVisible();
                    videoPanel.SetActive(true);
                    if (_popupMotion) await _popupMotion.SetVisibleAsync(true, _screenCts.Token);
                    else endPopup.SetActive(true);
                    Debug.LogError("[ResultVideo] Playback failed. Press the button to retry or hold for 5 seconds to return home.", this);
                    Debug.LogException(ex, this);
                }
            }
            finally
            {
                visualLock?.Dispose();
                _awaitingFirstFrame = false;
                if (player)
                {
                    player.sendFrameReadyEvents = false;
                    player.skipOnDrop = originalSkipOnDrop;
                }
                // Keep the final frame behind the end overlay; the screen owns it until return/disable.
                if (this && State != ScreenState.Finished)
                {
                    if (State != ScreenState.Failed) RestoreAdvertisement();
                    ReleaseMedia();
                }
                _timer?.Resume();
                if (_manager && !_manager.IsTransitioning) _manager.Presentation.ClearBlackout();
                if (_playbackCts == source) _playbackCts = null;
                source.Dispose();
            }
        }

        private void ThrowVideoError()
        {
            if (_videoError != null) throw new InvalidOperationException("Video decoder error: " + _videoError);
        }

        private void OnVideoEnded(VideoPlayer _) => _ended = true;
        private void OnVideoError(VideoPlayer _, string message) => _videoError = message;

        private void OnFrameReady(VideoPlayer sender, long frame)
        {
            if (!_awaitingFirstFrame) return;
            _awaitingFirstFrame = false;
            _firstFrame = frame;
            sender.Pause();
            sender.sendFrameReadyEvents = false;
            _firstFrameReady = true;
        }

        private void RestoreAudio()
        {
            if (_audioMutes == null) return;
            for (ushort track = 0; track < _audioMutes.Length; track++) player.SetDirectAudioMute(track, _audioMutes[track]);
            _audioMutes = null;
        }

        private void RestoreAdvertisement()
        {
            if (playbackHint) playbackHint.SetActive(false);
            _advertisementMotion?.SnapVisible();
            advertisement.SetActive(true);
            videoImage.enabled = false;
            _videoMotion?.PrepareHidden();
            videoPanel.SetActive(false);
            if (isActiveAndEnabled && (!_manager || !_manager.IsTransitioning)) productHighlight?.ResumeAmbient();
        }

        /// <summary>Reader-presence sink. Equipment adapters should update the shared RfidReaderHub.</summary>
        public void SetBlockPresent(int slot, bool present)
        {
            if (slot < 0 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
            if (present) _remainingBlocks |= 1 << slot;
            else _remainingBlocks &= ~(1 << slot);
            _timer?.ResetTimer();
            if (_remainingBlocks == 0 && State == ScreenState.Finished) _manager?.ReturnToIdleManually();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isFocused || State != ScreenState.Finished || (_manager && _manager.IsPresentationLocked)) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) RemoveReader(0);
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) RemoveReader(1);
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) RemoveReader(2);
            if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame) RemoveReader(3);
        }

        private void RemoveReader(int reader) { if (_readers) _readers.RemoveReader(reader); else SetBlockPresent(reader, false); }
#endif

        private void ReleaseMedia()
        {
            if (playbackHint) playbackHint.SetActive(false);
            videoImage.enabled = false;
            _awaitingFirstFrame = false;
            player.sendFrameReadyEvents = false;
            RestoreAudio();
            player.Stop();
            player.targetTexture = null;
            player.clip = null;
            videoImage.texture = null;
            if (_target) { _target.Release(); Destroy(_target); _target = null; }
            if (_clip) { Resources.UnloadAsset(_clip); _clip = null; }
        }

        private void OnDisable()
        {
            if (_manager) _manager.ShortButtonPressed -= Continue;
            player.loopPointReached -= OnVideoEnded;
            player.errorReceived -= OnVideoError;
            player.frameReady -= OnFrameReady;
            _popupMotion?.ResetHidden();
            _playbackCts?.Cancel();
            _screenCts?.Cancel();
            _screenCts?.Dispose();
            _screenCts = null;
            RestoreAdvertisement();
            ReleaseMedia();
            _timer?.Resume();
        }
    }
}
