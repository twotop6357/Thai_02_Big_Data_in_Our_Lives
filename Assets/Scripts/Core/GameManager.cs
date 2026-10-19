using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Exhibition.Input;
using MessagePipe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using VContainer;
using HuliacDev.App;
using HuliacDev.Core;
using HuliacDev.Data;
using HuliacDev.UI;
using HuliacDev.Utils;

namespace Core
{
    /// <summary>
    /// 프로젝트 전용 게임 매니저.
    /// 싱글톤 유지, 디버그 토글, Settings 비동기 로드는 GameManagerBase가 제공하며
    /// 프로젝트 고유의 전역 흐름 제어를 이곳에 추가함.
    /// </summary>
    public class GameManager : GameManagerBase
    {
        [SerializeField] private string idleScenePath = "Assets/Scenes/0_Idle.unity";
        [SerializeField] private string gameScenePath = "Assets/Scenes/1_Game.unity";
        [SerializeField] private SessionLanguage currentLanguage = SessionLanguage.Thai;

        private readonly ExhibitionButtonGesture _gesture = new ExhibitionButtonGesture();
        private ISubscriber<InactivityTimeoutEvent> _inactivityTimeoutSubscriber;
        private IDisposable _inactivityTimeoutSubscription;
        private IPublisher<MoveIdleEvent> _moveIdlePublisher;
        private InactivityTimer _inactivityTimer;
        private SoundManager _soundManager;
        private KeyboardButtonSimulator _keyboardSimulator;
        private CancellationTokenSource _sessionCts;
        private bool _buttonPressed;
        private bool _isIdle;
        private bool _inputReady;

        public SessionLanguage CurrentLanguage => currentLanguage;
        public string LanguageCode => currentLanguage == SessionLanguage.Thai ? "th" : "en";
        public bool HasLanguageSelection { get; private set; }
        public bool IsTransitioning { get; private set; }
        public bool IsInputReady => _inputReady;
        public double DoublePressIntervalSeconds => _gesture.DoublePressIntervalSeconds;

        // Future API/video tasks must use this plus their own screen lifetime token.
        public CancellationToken SessionCancellationToken =>
            _sessionCts != null ? _sessionCts.Token : new CancellationToken(true);

        /// <summary>
        /// GameManagerBase.Construct와 별개의 [Inject] 메서드. VContainer는 타입 계층의 각
        /// 레벨에서 선언된 [Inject] 메서드를 모두 수집해 주입하므로, 베이스 시그니처를 건드리지
        /// 않고 이 프로젝트에서만 필요한 의존성을 추가할 수 있음.
        /// </summary>
        [Inject]
        public void ConstructGameManager(ISubscriber<InactivityTimeoutEvent> inactivityTimeoutSubscriber,
            IPublisher<MoveIdleEvent> moveIdlePublisher, IObjectResolver resolver)
        {
            _inactivityTimeoutSubscriber = inactivityTimeoutSubscriber;
            _moveIdlePublisher = moveIdlePublisher;
            _inactivityTimer = resolver.ResolveOrDefault<InactivityTimer>();
            _soundManager = resolver.ResolveOrDefault<SoundManager>();
        }

        /// <summary>
        /// InactivityTimer의 타임아웃 이벤트를 구독함. move_idle_timeout 로그는 ApiManagerBase가
        /// 같은 이벤트를 자체 구독해 자동으로 보내므로, 여기서는 화면 복귀만 담당함.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _inactivityTimeoutSubscription = _inactivityTimeoutSubscriber?.Subscribe(_ => ReturnToFirstScreen());
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            _isIdle = SceneManager.GetActiveScene().path == idleScenePath;
            ResetButtonInput();
        }

        protected override void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _inactivityTimeoutSubscription?.Dispose();
            _inactivityTimeoutSubscription = null;
            ResetButtonInput();
            EndSession();
            base.OnDisable();
        }

        /// <summary>
        /// 최초 Idle 씬으로 복귀하며 세션과 입력 상태를 초기화함.
        /// <para>
        /// 수동 복귀는 ReturnToIdleManually를 사용함. 해당 경로만 MoveIdleEvent를 발행하며,
        /// 이 타임아웃 경로는 ApiManagerBase의 move_idle_timeout 기록과 중복 발행하지 않음.
        /// </para>
        /// </summary>
        protected virtual void ReturnToFirstScreen()
        {
            if (_isIdle || IsTransitioning) return;
            Debug.Log("[GameManager] Inactivity timeout. Returning to Idle.", this);
            BeginTransition(idleScenePath, false, SessionLanguage.Thai);
        }

        protected override void OnDestroy()
        {
            EndSession();
            base.OnDestroy();
        }

        protected override void OnSettingsLoaded(Settings loadedSettings)
        {
            base.OnSettingsLoaded(loadedSettings);
            LoadButtonSettingsAsync().Forget();
        }

        private async UniTask LoadButtonSettingsAsync()
        {
            try
            {
                // Provider supplies package settings first. Its concrete Settings type has no
                // extension fields, so our fields are read once separately from the same file.
                ButtonInputSettings inputSettings = await JsonLoader.LoadAsync<ButtonInputSettings>(
                    "Settings.json", this.GetCancellationTokenOnDestroy());
                if (!inputSettings.HasValidInterval)
                    Debug.LogWarning("[GameManager] buttonDoublePressIntervalSeconds must be 0.1-2 seconds. Using 0.4.", this);
                _gesture.DoublePressIntervalSeconds = inputSettings.HasValidInterval
                    ? inputSettings.buttonDoublePressIntervalSeconds
                    : ButtonInputSettings.DefaultDoublePressIntervalSeconds;
                _gesture.Reset();
                _inputReady = true;
                Debug.Log($"[GameManager] Button input ready. Double-press interval: {_gesture.DoublePressIntervalSeconds} seconds.", this);
            }
            catch (OperationCanceledException) { }
        }

        private void Update()
        {
            if (!_inputReady || IsTransitioning) return;
            ButtonGesture gesture = _gesture.Tick(_buttonPressed, _isIdle, Time.realtimeSinceStartupAsDouble);
            switch (gesture)
            {
                case ButtonGesture.SelectThai:
                    BeginTransition(gameScenePath, false, SessionLanguage.Thai);
                    break;
                case ButtonGesture.SelectEnglish:
                    BeginTransition(gameScenePath, false, SessionLanguage.English);
                    break;
                case ButtonGesture.ReturnToIdle:
                    ReturnToIdleManually();
                    break;
            }
        }

        /// <summary>Main-thread press/release state; future Arduino adapter uses this same API.</summary>
        public void SetButtonPressed(bool pressed)
        {
            _buttonPressed = pressed;
            if (pressed) _inactivityTimer?.ResetTimer();
        }

        public void ResetButtonInput()
        {
            _buttonPressed = false;
            _gesture.Reset();
        }

        public bool RegisterKeyboardSimulator(KeyboardButtonSimulator simulator)
        {
            if (_keyboardSimulator && _keyboardSimulator != simulator) return false;
            _keyboardSimulator = simulator;
            return true;
        }

        public void UnregisterKeyboardSimulator(KeyboardButtonSimulator simulator)
        {
            if (_keyboardSimulator != simulator) return;
            ResetButtonInput();
            _keyboardSimulator = null;
        }

        public void ReturnToIdleManually()
        {
            if (_isIdle || IsTransitioning) return;
            Debug.Log("[GameManager] Manual return to Idle requested.", this);
            BeginTransition(idleScenePath, true, SessionLanguage.Thai);
        }

        private void BeginTransition(string scenePath, bool manualReturn, SessionLanguage language)
        {
            if (IsTransitioning) return;
            if (!Application.CanStreamedLevelBeLoaded(scenePath))
            {
                Debug.LogError($"[GameManager] Scene is missing from Build Settings: {scenePath}", this);
                return;
            }
            IsTransitioning = true;
            _gesture.Reset();
            Debug.Log($"[GameManager] Scene transition started: {scenePath}", this);
            TransitionAsync(scenePath, manualReturn, language).Forget();
        }

        private async UniTask TransitionAsync(string scenePath, bool manualReturn, SessionLanguage language)
        {
            bool returning = scenePath == idleScenePath;
            try
            {
                EndSession();
                if (returning)
                {
                    StopSessionMedia();
                    ClearSelection();
                }
                else
                {
                    currentLanguage = language;
                    HasLanguageSelection = true;
                    _sessionCts = new CancellationTokenSource();
                }

                AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
                if (operation == null) throw new InvalidOperationException("Scene load did not start.");
                if (manualReturn)
                {
                    // A subscriber failure must not release the transition guard mid-load.
                    try { _moveIdlePublisher.Publish(new MoveIdleEvent()); }
                    catch (Exception exception) { Debug.LogException(exception, this); }
                }
                // A scene load cannot be undone: do not clear the guard on UI cancellation.
                await operation.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
                if (!returning)
                {
                    Debug.Log(language == SessionLanguage.Thai
                        ? "[GameManager] Game started. Language: Thai (th)."
                        : "[GameManager] Game started. Language: English (en).", this);
                }
                else Debug.Log("[GameManager] Returned to Idle. Session cleared.", this);
            }
            catch (OperationCanceledException) { Debug.Log($"[GameManager] Scene transition cancelled: {scenePath}", this); }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                if (!returning)
                {
                    EndSession();
                    ClearSelection();
                }
            }
            finally
            {
                if (this)
                {
                    _inactivityTimer?.Resume();
                    _gesture.Reset();
                    IsTransitioning = false;
                }
            }
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _isIdle = next.path == idleScenePath;
            _gesture.Reset();
            if (_isIdle)
            {
                EndSession();
                ClearSelection();
                _inactivityTimer?.Resume();
            }
        }

        private void ClearSelection()
        {
            currentLanguage = SessionLanguage.Thai;
            HasLanguageSelection = false;
        }

        private void EndSession()
        {
            CancellationTokenSource source = _sessionCts;
            _sessionCts = null;
            if (source == null) return;
            try { source.Cancel(); }
            catch (Exception exception) { Debug.LogException(exception, this); }
            finally { source.Dispose(); }
        }

        private void StopSessionMedia()
        {
            _soundManager?.StopBGM();
            _soundManager?.StopSFX();
            // One-off allocations only on return, never in the input hot path.
            foreach (VideoPlayer video in FindObjectsByType<VideoPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                video.Stop();
            foreach (AudioSource audio in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                audio.Stop();
        }
    }
}
