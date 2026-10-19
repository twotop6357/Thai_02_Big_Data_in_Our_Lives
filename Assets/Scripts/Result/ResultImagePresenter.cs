using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.Result
{
    /// <summary>Shows the generated image handed over by GameManager. The texture stays owned by GameManager.</summary>
    [DisallowMultipleComponent]
    public sealed class ResultImagePresenter : MonoBehaviour
    {
        [SerializeField] private Image resultImage;

        private Sprite _originalSprite, _sprite;

        private void Awake()
        {
            if (resultImage) _originalSprite = resultImage.sprite;
        }

        private void Start()
        {
            if (!resultImage)
            {
                Debug.LogError("[ResultImagePresenter] Assign ResultImage.", this);
                return;
            }
            // One startup lookup, same as the other scene components.
            GameManager manager = FindFirstObjectByType<GameManager>();
            Texture2D texture = manager ? manager.ResultTexture : null;
            if (!texture)
            {
                Debug.LogWarning("[ResultImagePresenter] No generated image is available. Start from Idle and generate an image in Game.", this);
                return;
            }
            _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
            resultImage.sprite = _sprite;
            resultImage.preserveAspect = true;
            Debug.Log("[ResultImagePresenter] Generated image displayed.", this);
        }

        private void OnDestroy()
        {
            if (resultImage) resultImage.sprite = _originalSprite;
            if (_sprite) Destroy(_sprite);
            _sprite = null;
        }
    }
}
