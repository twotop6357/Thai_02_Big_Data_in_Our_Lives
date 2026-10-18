using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    /// <summary>Idle opt-in keyboard adapter. Persists for the return button in later scenes.</summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class KeyboardButtonSimulator : MonoBehaviour
    {
        private GameManager _manager;
        private bool _waitForRelease = true;
        private bool _hasFocus = true;
        private bool _registered;

        private void Start()
        {
            // One startup lookup: this scene adapter does not require a DI registration.
            _manager = FindFirstObjectByType<GameManager>();
            if (!_manager)
            {
                Debug.LogError("[KeyboardButtonSimulator] GameLifetimeScope did not create GameManager.", this);
                enabled = false;
                return;
            }
            if (!_manager.RegisterKeyboardSimulator(this))
            {
                Destroy(gameObject);
                return;
            }
            _registered = true;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() => _waitForRelease = true;

        private void Update()
        {
            if (!_registered || !_manager || !_hasFocus) return;
            Keyboard keyboard = Keyboard.current;
            bool pressed = keyboard != null && (keyboard.digit1Key.isPressed || keyboard.numpad1Key.isPressed);
            if (_waitForRelease)
            {
                if (!pressed) _waitForRelease = false;
                return;
            }
            _manager.SetButtonPressed(pressed);
        }

        private void OnDisable()
        {
            if (_registered && _manager) _manager.ResetButtonInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            _waitForRelease = true;
            // Unity may send focus callbacks to disabled behaviours too. A disabled keyboard
            // adapter must not clear input supplied by a different (future hardware) adapter.
            if (_registered && isActiveAndEnabled && _manager) _manager.ResetButtonInput();
        }

        private void OnDestroy()
        {
            if (_registered && _manager) _manager.UnregisterKeyboardSimulator(this);
        }
    }
}
