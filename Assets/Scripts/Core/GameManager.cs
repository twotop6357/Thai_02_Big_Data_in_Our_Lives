using System;
using MessagePipe;
using UnityEngine.SceneManagement;
using VContainer;
using HuliacDev.App;
using HuliacDev.Core;

namespace Core
{
    /// <summary>
    /// 프로젝트 전용 게임 매니저.
    /// 싱글톤 유지, 디버그 토글, Settings 비동기 로드는 GameManagerBase가 제공하며
    /// 프로젝트 고유의 전역 흐름 제어를 이곳에 추가함.
    /// </summary>
    public class GameManager : GameManagerBase
    {
        private ISubscriber<InactivityTimeoutEvent> _inactivityTimeoutSubscriber;
        private IDisposable _inactivityTimeoutSubscription;

        /// <summary>
        /// GameManagerBase.Construct와 별개의 [Inject] 메서드. VContainer는 타입 계층의 각
        /// 레벨에서 선언된 [Inject] 메서드를 모두 수집해 주입하므로, 베이스 시그니처를 건드리지
        /// 않고 이 프로젝트에서만 필요한 의존성을 추가할 수 있음.
        /// </summary>
        [Inject]
        public void ConstructGameManager(ISubscriber<InactivityTimeoutEvent> inactivityTimeoutSubscriber)
        {
            _inactivityTimeoutSubscriber = inactivityTimeoutSubscriber;
        }

        /// <summary>
        /// InactivityTimer의 타임아웃 이벤트를 구독함. move_idle_timeout 로그는 ApiManagerBase가
        /// 같은 이벤트를 자체 구독해 자동으로 보내므로, 여기서는 화면 복귀만 담당함.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _inactivityTimeoutSubscription = _inactivityTimeoutSubscriber?.Subscribe(_ => ReturnToFirstScreen());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _inactivityTimeoutSubscription?.Dispose();
            _inactivityTimeoutSubscription = null;
        }

        /// <summary>
        /// 최초 화면으로 되돌아갈 때 호출되는 훅. 화면/패널 전환 시스템이 아직 없어 기본
        /// 구현은 현재 씬을 재로드하는 최소 동작만 수행하며, 전환 시스템이 생기면 프로젝트에서
        /// override해 교체할 것.
        /// <para>
        /// 수동 복귀 경로(예: 콘텐츠 종료 버튼)를 추가할 때는 이 메서드를 호출하기 전후로
        /// IPublisher&lt;MoveIdleEvent&gt;.Publish(new MoveIdleEvent())를 직접 호출해 move_idle
        /// 로그를 보낼 것. 이 메서드 자체는 로그를 보내지 않음 — 타임아웃 경로(move_idle_timeout,
        /// ApiManagerBase가 InactivityTimeoutEvent를 자체 구독해 자동 전송)와 이 메서드를
        /// 공유하므로, 여기서 MoveIdleEvent까지 함께 발행하면 타임아웃 복귀 때도 move_idle이
        /// 중복으로 나가게 됨.
        /// </para>
        /// </summary>
        protected virtual void ReturnToFirstScreen()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
