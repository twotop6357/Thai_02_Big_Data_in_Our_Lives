using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Exhibition.Input;
using Exhibition.Blocks;
using Exhibition.UI;
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
        [SerializeField] private string resultScenePath = "Assets/Scenes/2_Result.unity";
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
        private ComfyServerLauncher _comfyServer;
        private bool _isGame;
        private Texture2D _resultTexture;
        private LocalizedImageSet _localizedUI;
        private UIPresentationTransitions _presentation;
        public UIPresentationTransitions Presentation => _presentation ? _presentation :
            (_presentation = GetComponent<UIPresentationTransitions>() ?? gameObject.AddComponent<UIPresentationTransitions>());
        public bool IsPresentationLocked => _presentation && _presentation.IsInputLocked;
        private AudioManager _audio;
        public AudioManager Audio
        {
            get
            {
                if (!_audio && !TryGetComponent(out _audio)) _audio = gameObject.AddComponent<AudioManager>();
                return _audio;
            }
        }

        public event Action ShortButtonPressed;
        public bool IsLanguageChanging { get; private set; }
        public bool IsTitleScreen => _isIdle;

        public SessionLanguage CurrentLanguage => currentLanguage;
        public string LanguageCode => currentLanguage == SessionLanguage.Thai ? "th" : "en";
        public bool HasLanguageSelection { get; private set; }
        public bool IsTransitioning { get; private set; }
        public bool IsInputReady => _inputReady && _comfyServer && _comfyServer.IsReady;

        public void ConfigureComfyServer(ComfyServerLauncher server)
        {
            _comfyServer = server;
            ResetButtonInput();
        }

        /// <summary>Generated image owned by this manager until the session returns to Idle.</summary>
        public Texture2D ResultTexture => _resultTexture;
        public bool HasResultPalette { get; private set; }
        public Color ResultPrimaryColor { get; private set; }
        public Color ResultSecondaryColor { get; private set; }

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
            _isGame = SceneManager.GetActiveScene().path == gameScenePath;
            _localizedUI = FindFirstObjectByType<LocalizedImageSet>();
            if (_localizedUI) _localizedUI.BindInputManager(this);
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
            BeginTransition(idleScenePath, false, currentLanguage);
        }

        protected override void OnDestroy()
        {
            EndSession();
            ClearResultTexture();
            base.OnDestroy();
        }

        protected override void OnSettingsLoaded(Settings loadedSettings)
        {
            base.OnSettingsLoaded(loadedSettings);
            _gesture.Reset();
            _inputReady = true;
            Debug.Log("[GameManager] Button settings loaded. Input also requires a ready ComfyUI server.", this);
        }

        private void Update()
        {
            if (!IsInputReady)
            {
                // Discard pending gestures. A held button must be released after the server is ready.
                _gesture.Reset();
                return;
            }
            if (IsTransitioning || IsPresentationLocked) return;
            ButtonGesture gesture = _gesture.Tick(_buttonPressed, _isIdle, Time.realtimeSinceStartupAsDouble);
            switch (gesture)
            {
                case ButtonGesture.StartSession:
                    if (!IsLanguageChanging && (!_localizedUI || _localizedUI.IsReady))
                        BeginTransition(gameScenePath, false, currentLanguage);
                    break;
                case ButtonGesture.ToggleLanguage:
                    ToggleTitleLanguageAsync().Forget();
                    break;
                case ButtonGesture.ReturnToIdle:
                    ReturnToIdleManually();
                    break;
                case ButtonGesture.ShortPress:
                    if (ShortButtonPressed != null) ShortButtonPressed.Invoke();
                    else TryOpenResult();
                    break;
            }
        }

        private async UniTask ToggleTitleLanguageAsync()
        {
            if (!_isIdle || IsTransitioning || IsLanguageChanging || !_localizedUI || !_localizedUI.IsReady) return;
            IsLanguageChanging = true;
            Audio.PlayLanguage();
            SessionLanguage next = currentLanguage == SessionLanguage.Thai ? SessionLanguage.English : SessionLanguage.Thai;
            try
            {
                if (await _localizedUI.ApplyLanguageAsync(next, true))
                {
                    currentLanguage = next;
                    Debug.Log("[GameManager] Title language changed to " + LanguageCode + ".", this);
                }
            }
            finally { IsLanguageChanging = false; }
        }

        public void TryOpenResult()
        {
            if (_isGame && _resultTexture && !IsTransitioning)
                BeginTransition(resultScenePath, false, currentLanguage);
        }

        /// <summary>Main-thread press/release state; future Arduino adapter uses this same API.</summary>
        public void SetButtonPressed(bool pressed)
        {
            _buttonPressed = pressed;
            if (pressed && IsInputReady) _inactivityTimer?.ResetTimer();
        }

        /// <summary>
        /// Takes ownership of a generated image for the Result scene. The previous image is destroyed.
        /// Ignored outside an active Game session so a late result cannot leak into Idle.
        /// </summary>
        public bool SetResultTexture(Texture2D texture, Color? primaryColor = null, Color? secondaryColor = null)
        {
            if (!texture || !HasLanguageSelection || !_isGame || IsTransitioning) return false;
            if (_resultTexture && _resultTexture != texture) Destroy(_resultTexture);
            _resultTexture = texture;
            HasResultPalette = primaryColor.HasValue && secondaryColor.HasValue;
            ResultPrimaryColor = primaryColor ?? Color.white;
            ResultSecondaryColor = secondaryColor ?? Color.white;
            return true;
        }

        public void ClearResultTexture()
        {
            if (_resultTexture) Destroy(_resultTexture);
            _resultTexture = null;
            HasResultPalette = false;
            ResultPrimaryColor = ResultSecondaryColor = Color.white;
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
            BeginTransition(idleScenePath, true, currentLanguage);
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
            if (scenePath == idleScenePath) Audio.PlayHome();
            else Audio.PlayConfirm();
            TransitionAsync(scenePath, manualReturn, language).Forget();
        }

        private async UniTask TransitionAsync(string scenePath, bool manualReturn, SessionLanguage language)
        {
            bool returning = scenePath == idleScenePath;
            // Game -> Result keeps the same visitor session, language and generated image.
            bool continuing = scenePath == resultScenePath;
            using var presentationLock = Presentation.LockInput();
            bool presentationCompleted = false;
            try
            {
                await Presentation.CoverAsync(this.GetCancellationTokenOnDestroy());
                if (!continuing) EndSession();
                if (returning)
                {
                    StopSessionMedia();
                    ClearSelection();
                }
                else if (!continuing)
                {
                    ClearResultTexture();
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
                if (continuing) Debug.Log("[GameManager] Result scene loaded.", this);
                else if (!returning)
                {
                    Debug.Log(language == SessionLanguage.Thai
                        ? "[GameManager] Game started. Language: Thai (th)."
                        : "[GameManager] Game started. Language: English (en).", this);
                }
                else Debug.Log("[GameManager] Returned to Idle. Session cleared.", this);
                await Presentation.RevealReadySceneAsync(this.GetCancellationTokenOnDestroy());
                presentationCompleted = true;
            }
            catch (OperationCanceledException) { Debug.Log($"[GameManager] Scene transition cancelled: {scenePath}", this); }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                if (!returning && !continuing)
                {
                    EndSession();
                    ClearSelection();
                }
            }
            finally
            {
                if (this)
                {
                    if (!presentationCompleted) Presentation.RestoreVisibleScene();
                    Presentation.ClearBlackout();
                    _inactivityTimer?.Resume();
                    _gesture.Reset();
                    IsTransitioning = false;
                }
            }
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _isIdle = next.path == idleScenePath;
            _isGame = next.path == gameScenePath;
            _localizedUI = FindFirstObjectByType<LocalizedImageSet>();
            if (_localizedUI) _localizedUI.BindInputManager(this);
            _gesture.Reset();
            if (_isIdle)
            {
                EndSession();
                ClearSelection();
                ClearResultTexture();
                _inactivityTimer?.Resume();
            }
        }

        private void ClearSelection()
        {
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
                if (!Audio.Owns(audio)) audio.Stop();
        }
    }
}
