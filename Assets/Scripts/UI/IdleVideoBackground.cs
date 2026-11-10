using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Exhibition.UI
{
    /// <summary>
    /// Title background video (c02), looped and silent. Sits right above the still Intro_bg, below the logo
    /// and every other title element. The image stays hidden until the first frame is decoded, so the still
    /// background shows while loading or if the video fails.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RawImage), typeof(VideoPlayer))]
    public sealed class IdleVideoBackground : MonoBehaviour
    {
        [SerializeField] private VideoClip clip;
        private VideoPlayer _player;
        private RawImage _image;
        private RenderTexture _target;

        public bool IsShowing => _image && _image.enabled;
        public VideoPlayer Player => _player;

        private void OnEnable()
        {
            _image = GetComponent<RawImage>();
            _player = GetComponent<VideoPlayer>();
            _image.enabled = false;
            _image.raycastTarget = false;
            if (!clip)
            {
                Debug.LogError("[IdleVideoBackground] Video clip is not assigned. Showing the still background.", this);
                return;
            }
            _target = new RenderTexture((int)clip.width, (int)clip.height, 0, RenderTextureFormat.ARGB32) { name = "IdleVideoBackground" };
            _target.Create();
            _image.texture = _target;
            _player.playOnAwake = false;
            _player.source = VideoSource.VideoClip;
            _player.clip = clip;
            _player.isLooping = true;
            _player.skipOnDrop = true;
            _player.waitForFirstFrame = true;
            _player.audioOutputMode = VideoAudioOutputMode.None;
            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.targetTexture = _target;
            // Only the first frame is reported; looping playback raises no per-frame callbacks.
            _player.sendFrameReadyEvents = true;
            _player.frameReady += OnFrameReady;
            _player.errorReceived += OnError;
            _player.Play();
        }

        private void OnFrameReady(VideoPlayer source, long frame)
        {
            source.sendFrameReadyEvents = false;
            source.frameReady -= OnFrameReady;
            if (_image) _image.enabled = true;
        }

        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError("[IdleVideoBackground] Background video failed. Showing the still background. " + message, this);
            if (_image) _image.enabled = false;
        }

        private void OnDisable()
        {
            if (_player)
            {
                _player.frameReady -= OnFrameReady;
                _player.errorReceived -= OnError;
                _player.sendFrameReadyEvents = false;
                _player.Stop();
                _player.targetTexture = null;
            }
            if (_image)
            {
                _image.enabled = false;
                _image.texture = null;
            }
            if (_target)
            {
                _target.Release();
                Destroy(_target);
                _target = null;
            }
        }
    }
}
