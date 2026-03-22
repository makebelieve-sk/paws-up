# Step 17 — Butcher Shop Investigation Content [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 17 of 21. Depends on: Steps 10-16.

This is the core gameplay loop: Pierre investigates Jean's shop, examines objects, uses smell sense to find clues, and discovers the blue cloth.

## Task
Configure all interactable objects in the butcher shop with examination texts, smell highlights, scent trails, and quest progression.

## 1. Investigation Dialogues

Create `Assets/_PawsUp/Data/Dialogues/Investigation_Shop.yarn`:

```yarn
title: Pierre_Examine_DisplayCase
---
Пьер: Витрина совершенно пуста. Ни одной колбаски.
Пьер: Здесь стояли те самые трюфельные колбасы — 24 штуки.
Пьер: *принюхивается* Запах ещё свежий. Это было сегодня ночью.
===

title: Pierre_Examine_CashRegister
---
Пьер: Касса не тронута. Деньги на месте.
Пьер: Значит, вор пришёл не за деньгами. Его интересовали только колбасы.
Пьер: Целенаправленная кража... Кто-то знал, ЧТО именно здесь хранится.
===

title: Pierre_Examine_OrderBook
---
Пьер: Книга заказов... Последняя запись: "24 колбаски с трюфелями — фестиваль".
Пьер: Трюфели... Дорогой ингредиент. Не каждый может их достать.
Пьер: Кто в городе торгует трюфелями? Надо подумать...
<<set_flag checked_order_book>>
===

title: Pierre_Examine_BackDoor
---
Пьер: Задняя дверь заперта изнутри. Засов на месте.
Пьер: Но... чувствую сквозняк. Откуда-то тянет.
Пьер: *смотрит вниз* Щель под дверью. Для человека — мелочь. Для кота — зацепка.
Пьер: Может, стоит принюхаться повнимательнее...
===

title: Pierre_Examine_BackDoorGap_NoSmell
---
Пьер: Щель под дверью. Узкая, но что-то в ней есть...
Пьер: Глазами не разглядеть. Но нос подсказывает — здесь что-то важное.
Пьер: Нужно использовать кошачий нюх! Нажми Q.
===

title: Pierre_Found_BlueCloths
---
Пьер: Что это?! Клочок синей ткани!
Пьер: Застрял в щели под дверью. Вор протаскивал колбасы через подземный ход!
Пьер: Синяя ткань... Грубая, прочная. Похоже на попону или форменную одежду.
Пьер: Это первая настоящая улика. Нужно выяснить, кому принадлежит эта ткань!
<<set_flag cloth_found>>
<<complete_objective main_q1 find_clue>>
===
```

## 2. Investigation Controller

Create `Assets/_PawsUp/Scripts/Interaction/InvestigationController.cs`:

```csharp
using UnityEngine;
using PawsUp.Dialogue;
using PawsUp.Quest;
using PawsUp.SmellSense;

namespace PawsUp.Interaction
{
    /// <summary>
    /// Manages the investigation flow in the butcher shop.
    /// Attach to a manager object in the ButcherShop scene.
    /// </summary>
    public class InvestigationController : MonoBehaviour
    {
        [Header("Smell Sense Objects")]
        [SerializeField] private GameObject backDoorGapClue; // the pickup object
        [SerializeField] private GameObject scentTrailObject; // particle trail
        [SerializeField] private SmellHighlight gapHighlight;

        [Header("Quest")]
        [SerializeField] private string questId = "main_q1";
        [SerializeField] private string smellObjectiveId = "use_smell";

        private bool _firstSmellUsed;

        private void Start()
        {
            // Hide clue visuals until smell sense reveals them
            if (backDoorGapClue != null)
                backDoorGapClue.SetActive(false);

            // Listen for smell sense activation
            if (SmellSenseManager.Instance != null)
            {
                SmellSenseManager.Instance.OnActivated.AddListener(OnSmellActivated);
            }
        }

        private void OnSmellActivated()
        {
            if (!_firstSmellUsed && QuestManager.Instance.IsQuestActive(questId))
            {
                _firstSmellUsed = true;
                QuestManager.Instance.CompleteObjective(questId, smellObjectiveId);

                // Show the clue object (it has SmellHighlight, so it will glow)
                if (backDoorGapClue != null)
                    backDoorGapClue.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (SmellSenseManager.Instance != null)
            {
                SmellSenseManager.Instance.OnActivated.RemoveListener(OnSmellActivated);
            }
        }
    }
}
```

## 3. ExaminableObject Configurations (for step 21)

When setting up the ButcherShop scene, configure each interactable:

### DisplayCase
- Component: `ExaminableObject`
- prompt: "[E] Осмотреть витрину"
- examinationText: (will use Yarn node instead — see below)
- requiresSmellSense: false
- OnExamined → call DialogueManager.StartDialogue("Pierre_Examine_DisplayCase")

### CashRegister
- Component: `ExaminableObject`
- prompt: "[E] Осмотреть кассу"
- OnExamined → DialogueManager.StartDialogue("Pierre_Examine_CashRegister")

### OrderBook
- Component: `ExaminableObject`
- prompt: "[E] Прочитать книгу заказов"
- OnExamined → DialogueManager.StartDialogue("Pierre_Examine_OrderBook")

### BackDoor
- Component: `ExaminableObject`
- prompt: "[E] Осмотреть дверь"
- OnExamined → DialogueManager.StartDialogue("Pierre_Examine_BackDoor")

### BackDoorGap (THE CLUE!)
- Component: `PickupObject`
- prompt: "[E] Подобрать"
- itemData: clue_blue_cloth (SO from step 12)
- requiresSmellSense: true (only interactable during smell sense)
- Component: `SmellHighlight`
- highlightColor: yellow (#FFD700)
- hideWhenInactive: true (invisible until smell sense)
- OnPickedUp → DialogueManager.StartDialogue("Pierre_Found_BlueCloths")

## 4. ScentTrail Setup (for step 21)

Place a ScentTrail between the DisplayCase and BackDoorGap:
1. Create empty GO `ScentTrail_ShopClue`
2. Add child ParticleSystem (from ScentTrailParticles prefab)
3. Add `ScentTrail` component
4. Waypoints: DisplayCase position → middle of floor → BackDoorGap position
5. trailColor: warm orange (#FFA500)
6. This trail shows the "scent path" of the stolen sausages

## 5. ExaminableYarnBridge

Create `Assets/_PawsUp/Scripts/Interaction/ExaminableYarnBridge.cs`:

```csharp
using UnityEngine;
using PawsUp.Dialogue;

namespace PawsUp.Interaction
{
    /// <summary>
    /// ExaminableObject that triggers a Yarn dialogue node instead of plain text.
    /// </summary>
    public class ExaminableYarnBridge : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "[E] Осмотреть";
        [SerializeField] private string yarnNodeName;
        [SerializeField] private bool requiresSmellSense;
        [SerializeField] private bool oneTimeOnly;

        private bool _hasBeenExamined;

        public string GetInteractionPrompt() => prompt;

        public bool CanInteract()
        {
            if (oneTimeOnly && _hasBeenExamined) return false;
            if (requiresSmellSense)
            {
                var smell = SmellSense.SmellSenseManager.Instance;
                return smell != null && smell.IsActive;
            }
            return true;
        }

        public void Interact()
        {
            _hasBeenExamined = true;
            DialogueManager.Instance?.StartDialogue(yarnNodeName);
        }
    }
}
```

## 6. Full Investigation Flow

The complete player experience in the butcher shop:

1. **Enter shop** → quest objective "Войти в лавку" auto-completes (via ConditionalDoor)
2. **Examine DisplayCase** → Pierre comments on empty case
3. **Examine CashRegister** → Pierre notes money wasn't taken
4. **Examine OrderBook** → Pierre reads about truffle sausages, flag set
5. **Examine BackDoor** → Pierre notices draft, hints at smell sense
6. **Press Q (Smell Sense)** → world desaturates, objective "Использовать нюх" completes
7. **ScentTrail appears** → glowing particles from display case to gap under door
8. **BackDoorGap glows** → SmellHighlight active, object becomes visible
9. **Interact with gap** → PickupObject → clue_blue_cloth added to inventory
10. **Pierre's monologue** → "Клочок синей ткани!" → objective "Найти улику" completes
11. **New objective** → "Исследовать город" (next steps: talk to others, find more clues)

## Verification
- Enter shop → objective ticks off
- Examine each object → unique dialogue plays
- Press Q → desaturation, trail visible, gap glows
- Interact with gap during smell → cloth picked up → in inventory
- Quest updates: all objectives complete in order
- After pickup, cloth is gone (destroyed)
- Without smell sense, gap is not interactable
