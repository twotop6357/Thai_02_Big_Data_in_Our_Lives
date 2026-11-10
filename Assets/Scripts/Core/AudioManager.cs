using UnityEngine;

namespace Core
{
    /// <summary>Plays interface sounds from the persistent manager object, so they outlive scene loads.</summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        private const string ConfirmResource = "Audios/confirm", HomeResource = "Audios/home",
            SelectedResource = "Audios/selected", LanguageResource = "Audios/language";
        private AudioSource _source;
        private AudioClip _confirm, _home, _selected, _language;
        private int _selectedFrame = -1;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0;
            _confirm = Load(ConfirmResource);
            _home = Load(HomeResource);
            _selected = Load(SelectedResource);
            _language = Load(LanguageResource);
        }

        private AudioClip Load(string resource)
        {
            var clip = Resources.Load<AudioClip>(resource);
            if (!clip) Debug.LogError("[AudioManager] Sound is missing: Resources/" + resource + ".", this);
            return clip;
        }

        public void PlayConfirm() => Play(_confirm);
        public void PlayHome() => Play(_home);
        public void PlayLanguage() => Play(_language);

        /// <summary>Card recognized on a block screen. Several readers changing in one frame play it once.</summary>
        public void PlaySelected()
        {
            if (_selectedFrame == Time.frameCount) return;
            _selectedFrame = Time.frameCount;
            Play(_selected);
        }

        /// <summary>Lets the return-home media stop keep the home sound playing.</summary>
        public bool Owns(AudioSource source) => source == _source;

        private void Play(AudioClip clip)
        {
            if (!clip) return;
            _source.PlayOneShot(clip);
            Debug.Log("[AudioManager] Played " + clip.name + ".", this);
        }
    }
}
