# Step 18 — Scene Transitions [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 18 of 21. Depends on: Steps 08-11.

## Task
Create scene loading with fade transitions, portals, spawn points, and persistent managers.

## 1. SceneTransitionManager

Create `Assets/_PawsUp/Scripts/SceneManagement/SceneTransitionManager.cs`:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PawsUp.SceneManagement
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        public string PendingSpawnId { get; private set; }
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadScene(string sceneName, string spawnId = "default")
        {
            if (IsTransitioning) return;
            StartCoroutine(TransitionRoutine(sceneName, spawnId));
        }

        private IEnumerator TransitionRoutine(string sceneName, string spawnId)
        {
            IsTransitioning = true;
            PendingSpawnId = spawnId;

            // Fade to black
            yield return FadeRoutine(0f, 1f);

            // Load scene
            yield return SceneManager.LoadSceneAsync(sceneName);

            // Scene loaded callback will handle spawn point
            // Small delay to ensure scene is set up
            yield return new WaitForSeconds(0.1f);

            // Fade back in
            yield return FadeRoutine(1f, 0f);

            IsTransitioning = false;
        }

        private IEnumerator FadeRoutine(float from, float to)
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
            fadeCanvasGroup.blocksRaycasts = to > 0.5f;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(PendingSpawnId)) return;

            // Find the matching spawn point
            var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawnPoints)
            {
                if (sp.spawnId == PendingSpawnId)
                {
                    TeleportPlayer(sp.transform);
                    break;
                }
            }

            // Re-bind cameras
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CameraSystem.CameraManager.Instance?.BindToTarget(player.transform);
            }
        }

        private void TeleportPlayer(Transform spawnTransform)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = spawnTransform.position;
            player.transform.rotation = spawnTransform.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}
```

## 2. SpawnPoint

Create `Assets/_PawsUp/Scripts/SceneManagement/SpawnPoint.cs`:

```csharp
using UnityEngine;

namespace PawsUp.SceneManagement
{
    public class SpawnPoint : MonoBehaviour
    {
        public string spawnId = "default";

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, spawnId);
#endif
        }
    }
}
```

## 3. ScenePortal

Create `Assets/_PawsUp/Scripts/SceneManagement/ScenePortal.cs`:

```csharp
using UnityEngine;

namespace PawsUp.SceneManagement
{
    public class ScenePortal : MonoBehaviour, Interaction.IInteractable
    {
        [SerializeField] private string prompt = "[E] Перейти";
        [SerializeField] private string targetScene;
        [SerializeField] private string targetSpawnId = "default";
        [SerializeField] private bool useTrigger = true; // auto-transition on enter

        public string GetInteractionPrompt() => prompt;
        public bool CanInteract() => true;

        public void Interact()
        {
            DoTransition();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (useTrigger && other.CompareTag("Player"))
            {
                DoTransition();
            }
        }

        private void DoTransition()
        {
            if (SceneTransitionManager.Instance == null || SceneTransitionManager.Instance.IsTransitioning)
                return;

            SceneTransitionManager.Instance.LoadScene(targetScene, targetSpawnId);
        }
    }
}
```

## 4. Update ConditionalDoor (from step 16)

The ConditionalDoor already calls `SceneTransitionManager.Instance.LoadScene()` — it will now work with the real transition manager.

## 5. Persistent Player Setup

Create `Assets/_PawsUp/Scripts/Core/PersistentPlayer.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Core
{
    /// <summary>
    /// Makes the player persist across scene loads.
    /// Attach to the Pierre prefab.
    /// </summary>
    public class PersistentPlayer : MonoBehaviour
    {
        private static PersistentPlayer _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

## 6. Fade Canvas Setup (for step 21)

Create a Canvas that persists across scenes:
1. On the `--- MANAGERS ---` DontDestroyOnLoad object:
   - Add child Canvas (Screen Space - Overlay, Sort Order: 999)
   - Add child Image (color: black, stretch to fill)
   - Add `CanvasGroup` component to the Image
   - Alpha: 0, Blocks Raycasts: false
2. Assign this CanvasGroup to SceneTransitionManager.fadeCanvasGroup

## 7. Scene Portal Configuration (for step 21)

| Scene | Portal Object | Target Scene | Target Spawn |
|-------|--------------|-------------|-------------|
| PierresAttic | ExitPortal | CentralStreet | from_attic |
| CentralStreet | JeanShopDoor (ConditionalDoor) | ButcherShop | from_street |
| ButcherShop | ExitPortal_to_street | CentralStreet | from_shop |

## 8. Build Settings

Add scenes in order:
1. Bootstrap (index 0) — loads first
2. PierresAttic (index 1)
3. CentralStreet (index 2)
4. ButcherShop (index 3)

## 9. Bootstrap Loader

Create `Assets/_PawsUp/Scripts/Core/BootstrapLoader.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PawsUp.Core
{
    /// <summary>
    /// Placed in Bootstrap scene. Loads the first gameplay scene after managers init.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string firstScene = "PierresAttic";
        [SerializeField] private string firstSpawnId = "default";

        private void Start()
        {
            // All DontDestroyOnLoad managers are now initialized
            SceneManagement.SceneTransitionManager.Instance?.LoadScene(firstScene, firstSpawnId);
        }
    }
}
```

## Verification
- Start from Bootstrap → fade → PierresAttic loads → Pierre at spawn point
- Walk to exit → fade → CentralStreet loads → Pierre at from_attic spawn
- Talk to Jean → door unlocks → enter → fade → ButcherShop → Pierre at from_street
- Exit shop → back to street → Pierre at from_shop spawn
- Inventory and quest state persist across all transitions
- No duplicate Pierre or Managers after transitions
