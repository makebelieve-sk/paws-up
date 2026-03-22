# Step 14 — Cat Smell Sense Ability [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6, URP. Step 14 of 21. Depends on: Steps 03, 12.

The cat smell sense is Pierre's special ability: press Q to activate. The world desaturates, clues glow, scent trails become visible. Energy depletes over time.

## Task
Create SmellSenseManager, visual effects (URP Volume), highlight components, and scent trails.

## 1. SmellSenseManager

Create `Assets/_PawsUp/Scripts/SmellSense/SmellSenseManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.Events;
using PawsUp.Core;

namespace PawsUp.SmellSense
{
    public class SmellSenseManager : MonoBehaviour
    {
        public static SmellSenseManager Instance { get; private set; }

        [Header("Energy")]
        [SerializeField] private float maxEnergy = 10f;
        [SerializeField] private float drainRate = 1f;
        [SerializeField] private float regenRate = 0.5f;
        [SerializeField] private float regenDelay = 1f; // seconds after deactivation

        [Header("References")]
        [SerializeField] private UnityEngine.Rendering.Volume smellVolume;

        public float CurrentEnergy { get; private set; }
        public float MaxEnergy => maxEnergy;
        public float EnergyNormalized => CurrentEnergy / maxEnergy;
        public bool IsActive { get; private set; }

        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        private PawsUpInputs _input;
        private float _regenTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            CurrentEnergy = maxEnergy;
            _input = FindAnyObjectByType<PawsUpInputs>();
            if (smellVolume != null) smellVolume.weight = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                if (IsActive) Deactivate();
                return;
            }

            if (_input != null && _input.smellSense)
            {
                if (IsActive) Deactivate();
                else if (CurrentEnergy > 0.5f) Activate();
            }

            if (IsActive)
            {
                CurrentEnergy -= drainRate * Time.deltaTime;
                if (CurrentEnergy <= 0f)
                {
                    CurrentEnergy = 0f;
                    Deactivate();
                }

                // Lerp volume weight to 1
                if (smellVolume != null)
                    smellVolume.weight = Mathf.Lerp(smellVolume.weight, 1f, Time.deltaTime * 5f);
            }
            else
            {
                _regenTimer -= Time.deltaTime;
                if (_regenTimer <= 0f)
                {
                    CurrentEnergy = Mathf.Min(CurrentEnergy + regenRate * Time.deltaTime, maxEnergy);
                }

                // Lerp volume weight to 0
                if (smellVolume != null)
                    smellVolume.weight = Mathf.Lerp(smellVolume.weight, 0f, Time.deltaTime * 5f);
            }
        }

        private void Activate()
        {
            IsActive = true;
            _regenTimer = regenDelay;
            OnActivated?.Invoke();

            // Notify all SmellHighlight objects
            var highlights = FindObjectsByType<SmellHighlight>(FindObjectsSortMode.None);
            foreach (var h in highlights) h.SetHighlightActive(true);
        }

        private void Deactivate()
        {
            IsActive = false;
            _regenTimer = regenDelay;
            OnDeactivated?.Invoke();

            var highlights = FindObjectsByType<SmellHighlight>(FindObjectsSortMode.None);
            foreach (var h in highlights) h.SetHighlightActive(false);
        }
    }
}
```

## 2. SmellHighlight

Create `Assets/_PawsUp/Scripts/SmellSense/SmellHighlight.cs`:

```csharp
using UnityEngine;

namespace PawsUp.SmellSense
{
    /// <summary>
    /// Attach to objects that should glow when smell sense is active.
    /// Uses emission pulsing on the material.
    /// </summary>
    public class SmellHighlight : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.2f, 1f); // yellow glow
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMin = 0.3f;
        [SerializeField] private float pulseMax = 1.5f;
        [SerializeField] private bool hideWhenInactive = true;

        private Renderer _renderer;
        private Material _material;
        private bool _isHighlighted;
        private Color _originalEmission;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
            {
                _material = _renderer.material; // instance
                _originalEmission = _material.GetColor("_EmissionColor");
            }

            if (hideWhenInactive)
                gameObject.SetActive(false); // hidden until smell sense
        }

        public void SetHighlightActive(bool active)
        {
            _isHighlighted = active;

            if (hideWhenInactive)
                gameObject.SetActive(active);

            if (!active && _material != null)
            {
                _material.SetColor("_EmissionColor", _originalEmission);
                _material.DisableKeyword("_EMISSION");
            }
            else if (active && _material != null)
            {
                _material.EnableKeyword("_EMISSION");
            }
        }

        private void Update()
        {
            if (!_isHighlighted || _material == null) return;

            float pulse = Mathf.Lerp(pulseMin, pulseMax,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

            _material.SetColor("_EmissionColor", highlightColor * pulse);
        }
    }
}
```

## 3. ScentTrail

Create `Assets/_PawsUp/Scripts/SmellSense/ScentTrail.cs`:

```csharp
using UnityEngine;

namespace PawsUp.SmellSense
{
    /// <summary>
    /// A particle trail between waypoints, only visible during smell sense.
    /// Place this on an empty GO with a ParticleSystem child.
    /// </summary>
    public class ScentTrail : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Color trailColor = new Color(1f, 0.8f, 0.2f, 0.6f);
        [SerializeField] private float emissionRate = 15f;

        private void Awake()
        {
            if (particles == null)
                particles = GetComponentInChildren<ParticleSystem>();

            SetActive(false);

            // Listen to smell sense
            SmellSenseManager.Instance?.OnActivated.AddListener(() => SetActive(true));
            SmellSenseManager.Instance?.OnDeactivated.AddListener(() => SetActive(false));
        }

        private void Start()
        {
            // Also listen after Instance is ready
            if (SmellSenseManager.Instance != null)
            {
                SmellSenseManager.Instance.OnActivated.AddListener(() => SetActive(true));
                SmellSenseManager.Instance.OnDeactivated.AddListener(() => SetActive(false));
            }
        }

        private void SetActive(bool active)
        {
            if (particles == null) return;

            if (active)
            {
                particles.Play();
                var emission = particles.emission;
                emission.rateOverTime = emissionRate;
            }
            else
            {
                particles.Stop();
            }
        }
    }
}
```

## 4. URP Volume Profile Setup (instructions for human in step 21)

Create a Global Volume for the smell sense effect:
1. In any scene: Hierarchy → Volume → Global Volume
2. Name it `SmellSenseVolume`
3. Create a new Volume Profile: `Assets/_PawsUp/VFX/SmellSenseProfile.asset`
4. Add overrides:
   - **Color Adjustments:** Saturation = -80 (strong desaturation)
   - **Vignette:** Intensity = 0.35, Smoothness = 0.4
   - **Bloom:** Intensity = 0.5 (makes glowing objects stand out more)
5. Set Volume Weight to 0 (SmellSenseManager controls this)
6. Assign this Volume to `SmellSenseManager.smellVolume` field on Pierre

## 5. Particle System Setup (for ScentTrail)

Create a prefab `Assets/_PawsUp/Prefabs/VFX/ScentTrailParticles.prefab`:
- Shape: Edge (line between two points) or use waypoints
- Start Color: yellow-orange (#FFD700)
- Start Size: 0.05-0.1
- Start Lifetime: 1.5
- Emission Rate: 15
- Simulation Space: World
- Renderer → Material: use a soft particle material (Additive shader or URP Particles/Unlit)

## Verification
- Press Q → world desaturates (Volume weight → 1)
- Objects with SmellHighlight glow yellow and pulse
- Energy bar depletes
- Release Q (or energy runs out) → world returns to normal
- Energy regenerates after delay
- ScentTrail particles appear only during smell sense
- Objects with hideWhenInactive appear/disappear with smell sense
