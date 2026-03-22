# Step 20 — Audio Manager & Atmosphere [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 20 of 21. Depends on: Steps 08-10, 14, 18.

The project has existing audio assets in `Assets/Audio/` (Action, Ethereal, Suspenseful, Wind).

## Task
Create AudioManager, footstep system, and ambient sound zones.

## 1. AudioManager

Create `Assets/_PawsUp/Scripts/Audio/AudioManager.cs`:

```csharp
using System.Collections;
using UnityEngine;

namespace PawsUp.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSourceA;
        [SerializeField] private AudioSource bgmSourceB;
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField] private float crossfadeDuration = 1.5f;

        private AudioSource _activeBgm;
        private float _bgmVolume = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _activeBgm = bgmSourceA;
        }

        public void PlayBGM(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null) return;
            _bgmVolume = volume;

            if (_activeBgm.clip == clip && _activeBgm.isPlaying) return;

            StartCoroutine(CrossfadeBGM(clip, volume));
        }

        public void StopBGM(float fadeTime = 1f)
        {
            StartCoroutine(FadeOut(_activeBgm, fadeTime));
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, float volume)
        {
            var fadeOut = _activeBgm;
            var fadeIn = _activeBgm == bgmSourceA ? bgmSourceB : bgmSourceA;

            fadeIn.clip = newClip;
            fadeIn.volume = 0f;
            fadeIn.Play();

            float t = 0f;
            while (t < crossfadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float ratio = t / crossfadeDuration;
                fadeOut.volume = Mathf.Lerp(volume, 0f, ratio);
                fadeIn.volume = Mathf.Lerp(0f, volume, ratio);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = volume;
            _activeBgm = fadeIn;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVol = source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            source.Stop();
        }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = volume;
            _activeBgm.volume = volume;
        }
    }
}
```

## 2. SceneAudioLoader

Create `Assets/_PawsUp/Scripts/Audio/SceneAudioLoader.cs`:

```csharp
using UnityEngine;
using PawsUp.Core;

namespace PawsUp.Audio
{
    /// <summary>
    /// Place one in each scene. Loads BGM/ambient from SceneConfig.
    /// </summary>
    public class SceneAudioLoader : MonoBehaviour
    {
        [SerializeField] private SceneConfig sceneConfig;

        private void Start()
        {
            if (sceneConfig == null || AudioManager.Instance == null) return;

            if (sceneConfig.bgmClip != null)
                AudioManager.Instance.PlayBGM(sceneConfig.bgmClip, sceneConfig.bgmVolume);
        }
    }
}
```

## 3. FootstepSystem

Create `Assets/_PawsUp/Scripts/Audio/FootstepSystem.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Audio
{
    /// <summary>
    /// Attach to Pierre. Plays footstep sounds based on movement speed.
    /// </summary>
    public class FootstepSystem : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip[] footstepClips;

        [Header("Intervals")]
        [SerializeField] private float walkInterval = 0.5f;
        [SerializeField] private float runInterval = 0.3f;
        [SerializeField] private float crouchInterval = 0.8f;

        [Header("Settings")]
        [SerializeField] private float volume = 0.4f;

        private CharacterController _controller;
        private float _timer;
        private int _lastClipIndex = -1;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null || footstepClips.Length == 0) return;

            float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            if (speed < 0.2f || !_controller.isGrounded) return;

            float interval = speed > 4f ? runInterval :
                             speed < 1.5f ? crouchInterval : walkInterval;

            _timer += Time.deltaTime;
            if (_timer >= interval)
            {
                _timer = 0f;
                PlayFootstep();
            }
        }

        private void PlayFootstep()
        {
            // Avoid repeating same clip
            int index;
            do { index = Random.Range(0, footstepClips.Length); }
            while (index == _lastClipIndex && footstepClips.Length > 1);
            _lastClipIndex = index;

            AudioManager.Instance?.PlaySFXAtPoint(
                footstepClips[index], transform.position, volume);
        }
    }
}
```

## 4. SFX Collection

Create `Assets/_PawsUp/Scripts/Audio/SFXLibrary.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Audio
{
    /// <summary>
    /// Central place for all game SFX clips. Assign in Inspector.
    /// </summary>
    public class SFXLibrary : MonoBehaviour
    {
        public static SFXLibrary Instance { get; private set; }

        [Header("Interaction")]
        public AudioClip interact;
        public AudioClip pickup;
        public AudioClip doorOpen;

        [Header("Smell Sense")]
        public AudioClip smellActivate;
        public AudioClip smellDeactivate;

        [Header("Dialogue")]
        public AudioClip dialogueType; // per-character typing sound
        public AudioClip dialogueChoice;

        [Header("UI")]
        public AudioClip inventoryOpen;
        public AudioClip inventoryClose;
        public AudioClip menuClick;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

## 5. SceneConfig Audio Fields

The SceneConfig SO (from step 01) already has bgmClip and ambientClip fields. Create SceneConfig assets:

### PierresAttic_Config
- sceneName: PierresAttic
- displayName: Чердак Пьера
- cameraPreset: Indoor
- bgmClip: use `Assets/Audio/Ethereal_*` (calm, ambient)
- bgmVolume: 0.3

### CentralStreet_Config
- sceneName: CentralStreet
- displayName: Центральная улица
- cameraPreset: Exploration
- bgmClip: use `Assets/Audio/Suspenseful_*` (light investigation theme)
- bgmVolume: 0.4

### ButcherShop_Config
- sceneName: ButcherShop
- displayName: Лавка Жана
- cameraPreset: Indoor
- bgmClip: same as street or quieter variant
- bgmVolume: 0.25

## 6. Unity Setup (for step 21)

On `--- MANAGERS ---` object:
1. Add `AudioManager` component
2. Add 2 child AudioSources for BGM (loop = true, playOnAwake = false)
3. Add 1 child AudioSource for SFX (loop = false, playOnAwake = false)
4. Add `SFXLibrary` component (assign clips later — use placeholders from Assets/Audio/SoundFX/)
5. Add `FootstepSystem` to Pierre prefab (assign footstep clips)
6. In each scene: add empty GO with `SceneAudioLoader` + assign SceneConfig

## Verification
- Each scene plays its own BGM on load
- Switching scenes → BGM crossfades smoothly
- Pierre's footsteps play while moving, faster when running
- PlaySFX works for interaction/pickup sounds
- Pause → audio continues (unscaled time for BGM)
