using System;
using UnityEngine;

namespace Exhibition.UI
{
    [CreateAssetMenu(menuName = "Exhibition/UI Assets")]
    public sealed class ExhibitionUIAssets : ScriptableObject
    {
        [Serializable]
        public struct Entry { public string id; public Sprite sprite; }
        public Entry[] sprites;
        [TextArea] public string defaultThai, defaultEnglish;

        public Sprite Find(string id)
        {
            foreach (var entry in sprites)
                if (entry.id == id && entry.sprite) return entry.sprite;
            throw new ArgumentException("UI sprite is not registered: " + id);
        }
    }
}
