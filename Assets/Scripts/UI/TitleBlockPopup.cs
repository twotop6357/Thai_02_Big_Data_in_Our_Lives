using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Exhibition.Blocks;
using UnityEngine;

namespace Exhibition.UI
{
    /// <summary>
    /// Title screen: while any registered card is on a reader, shows the "remove all blocks" popup with the
    /// same sequence as the video end popup and blocks every button gesture (start and language).
    /// One image for both languages.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TitleBlockPopup : MonoBehaviour
    {
        [SerializeField] private UIPopupPresentation popup;
        private GameManager _manager;
        private RfidReaderHub _hub;
        private IDisposable _inputLock;
        private CancellationTokenSource _cts;
        private bool _requested;

        public bool IsRequested => _requested;
        public bool IsBlockingInput => _inputLock != null;

        private void OnEnable()
        {
            if (!popup)
            {
                Debug.LogError("[TitleBlockPopup] Assign the popup presentation.", this);
                enabled = false;
                return;
            }
            _cts = new CancellationTokenSource();
            _requested = false;
            popup.ResetHidden();
        }

        // The persistent manager and hub may be created after the first title scene enables this.
        private bool Resolve()
        {
            if (!_manager) _manager = FindFirstObjectByType<GameManager>();
            if (!_hub)
            {
                foreach (var hub in FindObjectsByType<RfidReaderHub>(FindObjectsSortMode.None))
                    if (hub.IsPrimary) { _hub = hub; break; }
            }
            return _manager && _hub;
        }

        private void LateUpdate()
        {
            if (!_manager || !_hub)
            {
                // Lookups run only until both are found.
                if (!Resolve()) return;
            }
            // Wait for the title entrance; the transition lock already blocks input meanwhile.
            bool wanted = _hub.HasPresentBlocks && _manager.IsTitleScreen && !_manager.IsTransitioning;
            if (wanted == _requested) return;
            _requested = wanted;
            if (wanted)
            {
                if (_inputLock == null) _inputLock = _manager.Presentation.LockInput();
                Debug.Log("[TitleBlockPopup] Blocks are on the readers. Showing the remove-blocks popup; button input is blocked.", this);
            }
            else Debug.Log("[TitleBlockPopup] Readers are empty. Hiding the popup.", this);
            AnimateAsync(wanted, _cts.Token).Forget();
        }

        private async UniTask AnimateAsync(bool visible, CancellationToken token)
        {
            await popup.SetVisibleAsync(visible, token);
            // Input stays blocked through the fade-out, and while a newer request shows it again.
            if (this && !_requested) ReleaseInput();
        }

        private void ReleaseInput()
        {
            var inputLock = _inputLock;
            _inputLock = null;
            inputLock?.Dispose();
        }

        private void OnDisable()
        {
            var source = _cts;
            _cts = null;
            source?.Cancel();
            source?.Dispose();
            _requested = false;
            ReleaseInput();
        }
    }
}
