using UnityEngine;

namespace Core
{
    /// <summary>Plays interface sounds from the persistent manager object, so they outlive scene loads.</summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        private const string ConfirmResource = "Audios/confirm", HomeResource = "Audios/home";
        private AudioSource _source;
        private AudioClip _confirm, _home;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0;
            _confirm = Load(ConfirmResource);
            _home = Load(HomeResource);
        }

        private AudioClip Load(string resource)
        {
            var clip = Resources.Load<AudioClip>(resource);
            if (!clip) Debug.LogError("[AudioManager] Sound is missing: Resources/" + resource + ".", this);
            return clip;
        }

        public void PlayConfirm() => Play(_confirm);
        public void PlayHome() => Play(_home);

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
