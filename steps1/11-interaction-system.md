# Step 11 — Interaction System [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6, URP. Step 11 of 21. Depends on: Step 03 (controller).

## Task
Create a modular interaction system: IInteractable interface, detection via OverlapSphere, UI prompt, and base implementations (Examinable, Pickup, Door).

## 1. IInteractable Interface

Create `Assets/_PawsUp/Scripts/Interaction/IInteractable.cs`:

```csharp
namespace PawsUp.Interaction
{
    public interface IInteractable
    {
        string GetInteractionPrompt(); // e.g. "[E] Examine", "[E] Talk"
        bool CanInteract();
        void Interact();
    }
}
```

## 2. InteractionDetector

Create `Assets/_PawsUp/Scripts/Interaction/InteractionDetector.cs`:

```csharp
using UnityEngine;
using PawsUp.Core;

namespace PawsUp.Interaction
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 2.5f;
        [SerializeField] private float dotThreshold = 0.3f;
        [SerializeField] private LayerMask interactionLayers = ~0;

        private IInteractable _currentTarget;
        private PawsUpInputs _input;
        private InteractionPromptUI _promptUI;

        public IInteractable CurrentTarget => _currentTarget;

        private readonly Collider[] _overlapResults = new Collider[20];

        private void Start()
        {
            _input = GetComponent<PawsUpInputs>();
            _promptUI = FindAnyObjectByType<InteractionPromptUI>();
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                ClearTarget();
                return;
            }

            FindBestInteractable();

            if (_currentTarget != null && _input.interact)
            {
                _currentTarget.Interact();
            }
        }

        private void FindBestInteractable()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, detectionRadius, _overlapResults, interactionLayers);

            IInteractable best = null;
            float bestScore = -1f;

            for (int i = 0; i < count; i++)
            {
                var interactable = _overlapResults[i].GetComponent<IInteractable>()
                    ?? _overlapResults[i].GetComponentInParent<IInteractable>();

                if (interactable == null || !interactable.CanInteract()) continue;

                Vector3 dir = (_overlapResults[i].transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dir);

                if (dot < dotThreshold) continue;

                float dist = Vector3.Distance(transform.position, _overlapResults[i].transform.position);
                float score = dot / Mathf.Max(dist, 0.1f);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = interactable;
                }
            }

            if (best != _currentTarget)
            {
                _currentTarget = best;
                if (_promptUI != null)
                {
                    if (_currentTarget != null)
                        _promptUI.Show(_currentTarget.GetInteractionPrompt(),
                            ((MonoBehaviour)_currentTarget).transform);
                    else
                        _promptUI.Hide();
                }
            }
        }

        private void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget = null;
                _promptUI?.Hide();
            }
        }
    }
}
```

## 3. InteractionPromptUI

Create `Assets/_PawsUp/Scripts/Interaction/InteractionPromptUI.cs`:

```csharp
using UnityEngine;
using TMPro;

namespace PawsUp.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);

        private Transform _target;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            Hide();
        }

        private void LateUpdate()
        {
            if (_target == null || !canvas.gameObject.activeSelf) return;

            Vector3 worldPos = _target.position + worldOffset;
            Vector3 screenPos = _cam.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0)
            {
                canvas.gameObject.SetActive(false);
                return;
            }

            canvas.gameObject.SetActive(true);
            promptText.transform.parent.position = screenPos;
        }

        public void Show(string text, Transform target)
        {
            _target = target;
            promptText.text = text;
            canvas.gameObject.SetActive(true);
        }

        public void Hide()
        {
            canvas.gameObject.SetActive(false);
            _target = null;
        }
    }
}
```

## 4. ExaminableObject

Create `Assets/_PawsUp/Scripts/Interaction/ExaminableObject.cs`:

```csharp
using UnityEngine;
using UnityEngine.Events;

namespace PawsUp.Interaction
{
    public class ExaminableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Осмотреть";
        [TextArea(2, 5)]
        [SerializeField] private string examinationText = "Ничего интересного.";
        [SerializeField] private bool requiresSmellSense;

        public UnityEvent OnExamined;

        public string GetInteractionPrompt() => prompt;

        public bool CanInteract()
        {
            if (requiresSmellSense)
            {
                // Will check SmellSenseManager in step 14
                var smell = FindAnyObjectByType<PawsUp.SmellSense.SmellSenseManager>();
                return smell != null && smell.IsActive;
            }
            return true;
        }

        public void Interact()
        {
            // Show text via dialogue or simple text popup
            Debug.Log($"[Examine] {examinationText}");
            OnExamined?.Invoke();

            // Will integrate with PierreMonologue in step 16
            var monologue = FindAnyObjectByType<PawsUp.Dialogue.PierreMonologue>();
            if (monologue != null)
                monologue.ShowThought(examinationText);
        }
    }
}
```

## 5. PickupObject

Create `Assets/_PawsUp/Scripts/Interaction/PickupObject.cs`:

```csharp
using UnityEngine;
using UnityEngine.Events;

namespace PawsUp.Interaction
{
    public class PickupObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Подобрать";
        [SerializeField] private PawsUp.Inventory.ItemData itemData;
        [SerializeField] private bool requiresSmellSense;

        public UnityEvent OnPickedUp;

        public string GetInteractionPrompt() => prompt;

        public bool CanInteract()
        {
            if (requiresSmellSense)
            {
                var smell = FindAnyObjectByType<PawsUp.SmellSense.SmellSenseManager>();
                return smell != null && smell.IsActive;
            }
            return true;
        }

        public void Interact()
        {
            if (itemData != null)
            {
                var inventory = PawsUp.Inventory.InventoryManager.Instance;
                if (inventory != null)
                {
                    inventory.AddItem(itemData);
                    Debug.Log($"[Pickup] {itemData.displayName}");
                }
            }

            OnPickedUp?.Invoke();

            // Simple scale-down animation then destroy
            StartCoroutine(PickupAnimation());
        }

        private System.Collections.IEnumerator PickupAnimation()
        {
            float t = 0;
            Vector3 startScale = transform.localScale;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.3f);
                transform.position += Vector3.up * Time.deltaTime * 2f;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
```

## 6. DoorTrigger (placeholder)

Create `Assets/_PawsUp/Scripts/Interaction/DoorTrigger.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Interaction
{
    public class DoorTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Войти";
        [SerializeField] private string targetScene;
        [SerializeField] private string targetSpawnId;

        public string GetInteractionPrompt() => prompt;
        public bool CanInteract() => true;

        public void Interact()
        {
            // Will call SceneTransitionManager in step 18
            Debug.Log($"[Door] → {targetScene} / {targetSpawnId}");
        }
    }
}
```

## 7. Unity Setup (for step 21)
- Create a Screen Space - Overlay Canvas with a child Panel containing TextMeshProUGUI for the prompt
- Add InteractionPromptUI component to this Canvas
- Add InteractionDetector to Pierre prefab
- Add ExaminableObject / PickupObject to interactable objects in scenes

## Verification
- Walk near an object with ExaminableObject → prompt "[E] Осмотреть" appears
- Press E → text logged to console (or shown in monologue system)
- Walk away → prompt disappears
- Multiple nearby objects → nearest one in front selected
- PickupObject → item animation → destroyed
