# Step 15 — Quest & Flag System [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 15 of 21. Depends on: Steps 12, 13.

## Task
Create a quest tracking system with objectives, flags, and Yarn Spinner integration.

## 1. QuestData ScriptableObject

Create `Assets/_PawsUp/Scripts/Quest/QuestData.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace PawsUp.Quest
{
    [System.Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string description;
        [HideInInspector] public bool isCompleted;
    }

    public enum QuestState
    {
        NotStarted,
        Active,
        Completed
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "PawsUp/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string questId;
        public string title;
        [TextArea(2, 4)]
        public string description;
        public List<QuestObjective> objectives = new List<QuestObjective>();
    }
}
```

## 2. QuestManager

Create `Assets/_PawsUp/Scripts/Quest/QuestManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PawsUp.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [SerializeField] private List<QuestData> allQuests = new List<QuestData>();

        private Dictionary<string, QuestState> _questStates = new();
        private Dictionary<string, QuestData> _questLookup = new();
        private Dictionary<string, bool> _flags = new();

        // Runtime copy of objectives (so we don't modify SO)
        private Dictionary<string, List<QuestObjective>> _runtimeObjectives = new();

        [Header("Events")]
        public UnityEvent<string> OnQuestStarted;       // questId
        public UnityEvent<string, string> OnObjectiveCompleted; // questId, objectiveId
        public UnityEvent<string> OnQuestCompleted;      // questId
        public UnityEvent<string> OnFlagSet;             // flagName

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Build lookup
            foreach (var q in allQuests)
            {
                _questLookup[q.questId] = q;
                _questStates[q.questId] = QuestState.NotStarted;
            }
        }

        // --- Quest Management ---

        public void StartQuest(string questId)
        {
            if (!_questLookup.ContainsKey(questId)) return;
            if (_questStates[questId] != QuestState.NotStarted) return;

            _questStates[questId] = QuestState.Active;

            // Create runtime copy of objectives
            var quest = _questLookup[questId];
            var runtimeObjs = new List<QuestObjective>();
            foreach (var obj in quest.objectives)
            {
                runtimeObjs.Add(new QuestObjective
                {
                    objectiveId = obj.objectiveId,
                    description = obj.description,
                    isCompleted = false
                });
            }
            _runtimeObjectives[questId] = runtimeObjs;

            OnQuestStarted?.Invoke(questId);
            Debug.Log($"[Quest] Started: {quest.title}");
        }

        public void CompleteObjective(string questId, string objectiveId)
        {
            if (!_runtimeObjectives.ContainsKey(questId)) return;

            var objectives = _runtimeObjectives[questId];
            foreach (var obj in objectives)
            {
                if (obj.objectiveId == objectiveId && !obj.isCompleted)
                {
                    obj.isCompleted = true;
                    OnObjectiveCompleted?.Invoke(questId, objectiveId);
                    Debug.Log($"[Quest] Objective completed: {obj.description}");

                    // Check if all objectives done
                    if (objectives.TrueForAll(o => o.isCompleted))
                    {
                        _questStates[questId] = QuestState.Completed;
                        OnQuestCompleted?.Invoke(questId);
                        Debug.Log($"[Quest] Completed: {_questLookup[questId].title}");
                    }
                    break;
                }
            }
        }

        public QuestState GetQuestState(string questId)
        {
            return _questStates.TryGetValue(questId, out var state) ? state : QuestState.NotStarted;
        }

        public QuestObjective GetCurrentObjective(string questId)
        {
            if (!_runtimeObjectives.ContainsKey(questId)) return null;
            foreach (var obj in _runtimeObjectives[questId])
                if (!obj.isCompleted) return obj;
            return null;
        }

        public bool IsQuestActive(string questId) => GetQuestState(questId) == QuestState.Active;
        public bool IsQuestCompleted(string questId) => GetQuestState(questId) == QuestState.Completed;

        // --- Flag System ---

        public void SetFlag(string flagName)
        {
            _flags[flagName] = true;
            OnFlagSet?.Invoke(flagName);
            Debug.Log($"[Flag] Set: {flagName}");
        }

        public bool HasFlag(string flagName)
        {
            return _flags.TryGetValue(flagName, out var val) && val;
        }

        public void ClearFlag(string flagName)
        {
            _flags[flagName] = false;
        }
    }
}
```

## 3. Create Quest ScriptableObject

In `Assets/_PawsUp/ScriptableObjects/Quests/`, create:

### main_q1 — "Расследовать кражу"
- questId: `main_q1`
- title: `Расследовать кражу`
- description: `Из лавки Жана пропали элитные колбасы с трюфелями. Нужно разобраться.`
- objectives:
  1. objectiveId: `talk_to_jean`, description: `Поговорить с Жаном`
  2. objectiveId: `enter_shop`, description: `Войти в лавку`
  3. objectiveId: `use_smell`, description: `Использовать кошачий нюх в лавке`
  4. objectiveId: `find_clue`, description: `Найти улику`
  5. objectiveId: `explore_town`, description: `Исследовать город`

## 4. Add QuestManager to Managers

In Bootstrap scene: add `QuestManager` component to `--- MANAGERS ---` object.
Assign `main_q1` SO to the `allQuests` list.

## Verification
- `QuestManager.Instance.StartQuest("main_q1")` → state = Active
- `QuestManager.Instance.CompleteObjective("main_q1", "talk_to_jean")` → objective marked
- `QuestManager.Instance.GetCurrentObjective("main_q1")` → returns next uncompleted
- `QuestManager.Instance.SetFlag("talked_to_jean")` → `HasFlag("talked_to_jean")` = true
- In Yarn: `<<set_flag talked_to_jean>>` works, `has_flag("talked_to_jean")` works
- Events fire correctly
