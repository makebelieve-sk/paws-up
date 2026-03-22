# Step 13 — Yarn Spinner Dialogue System [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6 (6000.3.11f1). Step 13 of 21. Depends on: Steps 11, 12.

## Task
Install Yarn Spinner, create a dialogue manager wrapper, dialogue UI, and custom Yarn commands/functions.

## 1. Install Yarn Spinner

Add to `Packages/manifest.json` — add the OpenUPM scoped registry AND the package:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "dev.yarnspinner"
      ]
    }
  ],
  "dependencies": {
    "dev.yarnspinner.unity": "2.4.2"
  }
}
```

Add this to the EXISTING manifest.json, merging with existing registries and dependencies.
After saving, Unity will download and compile Yarn Spinner.

Also add `"dev.yarnspinner.unity"` to the assembly definition references in `PawsUp.asmdef`:
```json
"references": [
    "Unity.InputSystem",
    "Unity.TextMeshPro",
    "Unity.Cinemachine",
    "YarnSpinner.Unity"
]
```

## 2. DialogueManager

Create `Assets/_PawsUp/Scripts/Dialogue/DialogueManager.cs`:

```csharp
using UnityEngine;
using Yarn.Unity;
using PawsUp.Core;

namespace PawsUp.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialogueRunner dialogueRunner;

        public event System.Action OnDialogueStart;
        public event System.Action OnDialogueEnd;

        public bool IsInDialogue { get; private set; }

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

        private void Start()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueStart.AddListener(HandleDialogueStart);
                dialogueRunner.onDialogueComplete.AddListener(HandleDialogueEnd);
            }
        }

        public void StartDialogue(string nodeName)
        {
            if (IsInDialogue || dialogueRunner == null) return;
            dialogueRunner.StartDialogue(nodeName);
        }

        private void HandleDialogueStart()
        {
            IsInDialogue = true;
            GameManager.Instance?.SetState(GameState.Dialogue);

            // Lock player movement
            var controller = FindAnyObjectByType<Player.PierreController>();
            if (controller != null) controller.IsMovementLocked = true;

            OnDialogueStart?.Invoke();
        }

        private void HandleDialogueEnd()
        {
            IsInDialogue = false;
            GameManager.Instance?.SetState(GameState.Playing);

            var controller = FindAnyObjectByType<Player.PierreController>();
            if (controller != null) controller.IsMovementLocked = false;

            OnDialogueEnd?.Invoke();
        }
    }
}
```

## 3. Custom Yarn Commands & Functions

Create `Assets/_PawsUp/Scripts/Dialogue/YarnCustomCommands.cs`:

```csharp
using UnityEngine;
using Yarn.Unity;

namespace PawsUp.Dialogue
{
    public class YarnCustomCommands : MonoBehaviour
    {
        // -- Functions (return values, used in conditionals) --

        [YarnFunction("has_item")]
        public static bool HasItem(string itemId)
        {
            var inv = Inventory.InventoryManager.Instance;
            return inv != null && inv.HasItem(itemId);
        }

        [YarnFunction("has_flag")]
        public static bool HasFlag(string flagName)
        {
            var quest = Quest.QuestManager.Instance;
            return quest != null && quest.HasFlag(flagName);
        }

        [YarnFunction("item_count")]
        public static int ItemCount(string itemId)
        {
            var inv = Inventory.InventoryManager.Instance;
            return inv != null ? inv.GetItemCount(itemId) : 0;
        }

        // -- Commands (actions, no return value) --

        [YarnCommand("set_flag")]
        public static void SetFlag(string flagName)
        {
            Quest.QuestManager.Instance?.SetFlag(flagName);
        }

        [YarnCommand("start_quest")]
        public static void StartQuest(string questId)
        {
            Quest.QuestManager.Instance?.StartQuest(questId);
        }

        [YarnCommand("complete_objective")]
        public static void CompleteObjective(string questId, string objectiveId)
        {
            Quest.QuestManager.Instance?.CompleteObjective(questId, objectiveId);
        }

        [YarnCommand("add_item")]
        public static void AddItem(string itemId)
        {
            var allItems = Resources.FindObjectsOfTypeAll<Inventory.ItemData>();
            foreach (var item in allItems)
            {
                if (item.itemId == itemId)
                {
                    Inventory.InventoryManager.Instance?.AddItem(item);
                    return;
                }
            }
            Debug.LogWarning($"[Yarn] Item not found: {itemId}");
        }

        [YarnCommand("show_thought")]
        public static void ShowThought(string text)
        {
            var monologue = FindAnyObjectByType<PierreMonologue>();
            monologue?.ShowThought(text);
        }
    }
}
```

## 4. TalkableNPC

Create `Assets/_PawsUp/Scripts/Dialogue/TalkableNPC.cs`:

```csharp
using UnityEngine;
using PawsUp.Interaction;

namespace PawsUp.Dialogue
{
    public class TalkableNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Поговорить";
        [SerializeField] private string yarnNodeName;
        [SerializeField] private string npcName;
        [SerializeField] private Sprite portrait;

        public Sprite Portrait => portrait;
        public string NpcName => npcName;

        public string GetInteractionPrompt() => prompt;
        public bool CanInteract() => !DialogueManager.Instance.IsInDialogue;

        public void Interact()
        {
            DialogueManager.Instance?.StartDialogue(yarnNodeName);
        }
    }
}
```

## 5. PierreMonologue

Create `Assets/_PawsUp/Scripts/Dialogue/PierreMonologue.cs`:

```csharp
using UnityEngine;
using TMPro;
using System.Collections;

namespace PawsUp.Dialogue
{
    /// <summary>
    /// Shows Pierre's internal thoughts as stylized text (italic, different color).
    /// Simpler than full dialogue — just a text popup.
    /// </summary>
    public class PierreMonologue : MonoBehaviour
    {
        [SerializeField] private GameObject thoughtPanel;
        [SerializeField] private TextMeshProUGUI thoughtText;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float typeSpeed = 0.03f;

        private Coroutine _currentRoutine;

        public void ShowThought(string text)
        {
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(ShowThoughtRoutine(text));
        }

        private IEnumerator ShowThoughtRoutine(string text)
        {
            thoughtPanel.SetActive(true);
            thoughtText.text = "";

            // Type out character by character
            foreach (char c in text)
            {
                thoughtText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(displayDuration);
            thoughtPanel.SetActive(false);
        }
    }
}
```

## 6. Test Yarn File

Create `Assets/_PawsUp/Data/Dialogues/Test.yarn`:

```yarn
title: Test_Hello
---
NPC: Привет, Пьер!
-> Привет!
    NPC: Как дела?
-> Некогда болтать.
    NPC: Ну ладно...
===
```

## 7. Unity Editor Setup (for step 21)

1. On `--- MANAGERS ---` object: add `DialogueRunner` component
   - Add a `Yarn Project` asset (Create → Yarn Spinner → Yarn Project)
   - Assign all .yarn files to the project
   - Add `InMemoryVariableStorage` component
2. Add `YarnCustomCommands` component (same object)
3. Create dialogue UI Canvas (bottom panel with portrait, name, text, choice buttons)
4. Add `DialogueManager` component, assign DialogueRunner reference
5. Set up a `LineView` and `OptionsListView` on the dialogue UI

## Verification
- Yarn Spinner compiles without errors
- Walking to NPC → "[E] Поговорить" → dialogue starts
- Text appears with typewriter effect
- Choices appear as buttons, clicking advances dialogue
- Player movement locked during dialogue, unlocked after
- `has_item("clue_blue_cloth")` works in yarn conditionals
- `<<set_flag talked_to_jean>>` command executes
