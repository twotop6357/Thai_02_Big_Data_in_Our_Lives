using TMPro;
using UnityEngine;

namespace Exhibition.Blocks
{
    /// <summary>Idle status line: visible only while ComfyUI is starting or unavailable, hidden once ready.</summary>
    [DisallowMultipleComponent]
    public sealed class ComfyStatusPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private string startingMessage = "Starting the AI image server...";
        [SerializeField] private string unavailableMessage = "The AI image server is not available.";

        private ComfyServerLauncher _launcher;

        private void Awake()
        {
            // Deactivating our own object would stop this presenter, so it must sit outside the text.
            if (statusText && statusText.gameObject == gameObject)
            {
                Debug.LogError("[ComfyStatusPresenter] Place this on a parent of ComfyUIStatusText, not on the text itself.", this);
                statusText = null;
                return;
            }
            // Hidden until the launcher reports a state worth showing; a running server never flashes text.
            if (statusText) statusText.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (!statusText)
            {
                Debug.LogError("[ComfyStatusPresenter] Assign ComfyUIStatusText.", this);
                return;
            }
            // One startup lookup; the launcher lives on the persistent GameManager root.
            _launcher = FindFirstObjectByType<ComfyServerLauncher>();
            if (!_launcher)
            {
                Debug.LogError("[ComfyStatusPresenter] ComfyServerLauncher is missing on the root.", this);
                return;
            }
            _launcher.StateChanged += Apply;
            Apply(_launcher.State);
        }

        private void OnDestroy()
        {
            if (_launcher) _launcher.StateChanged -= Apply;
        }

        private void Apply(ComfyServerState state)
        {
            bool show = state == ComfyServerState.Starting || state == ComfyServerState.Unavailable;
            if (show) statusText.text = state == ComfyServerState.Starting ? startingMessage : unavailableMessage;
            statusText.gameObject.SetActive(show);
        }
    }
}
