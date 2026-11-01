using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Exhibition.UI
{
    public static class UIMotion
    {
        public const float HintSeconds = .25f;
        public const float BackdropSeconds = .25f;
        public const float PopupSeconds = .3f;
        public const float PanelSeconds = .5f;
        public const float SceneSeconds = .5f;
        public const float ItemInterval = .2f;

        public static UniTask FadeAsync(CanvasGroup group, float alpha, float seconds, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return group.DOFade(alpha, seconds).SetEase(Ease.InOutSine).SetUpdate(true)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);
        }
    }
}
