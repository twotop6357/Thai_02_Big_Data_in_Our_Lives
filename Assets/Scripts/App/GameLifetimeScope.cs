using Core;
using VContainer;
using VContainer.Unity;
using HuliacDev.App;

namespace App
{
    /// <summary>
    /// 프로젝트 전용 루트 DI 스코프.
    /// 템플릿 RootLifetimeScope의 로깅·MessagePipe·설정 등록에 더해 씬 매니저 주입 등록을 담당함.
    /// </summary>
    public class GameLifetimeScope : RootLifetimeScope
    {
        /// <summary>
        /// 템플릿 기본 등록(로깅, MessagePipe, AppSettingsProvider)에 프로젝트 매니저 등록을 추가함.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            ConfigureManagers(builder);
        }

        /// <summary>
        /// 씬 계층에 배치된 매니저 컴포넌트를 컨테이너에 등록하여 메서드 주입이 동작하게 함.
        /// </summary>
        private void ConfigureManagers(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GameManager>();
        }
    }
}
