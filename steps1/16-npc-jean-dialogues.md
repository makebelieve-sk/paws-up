# Step 16 — NPC Jean, Rampier & Dialogues [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 16 of 21. Depends on: Steps 09 (street), 13 (dialogue), 15 (quests).

## Task
Create Yarn dialogue files for Jean and Rampier, create ConditionalDoor, and set up the quest flow on the street.

## 1. Jean's Dialogue

Create `Assets/_PawsUp/Data/Dialogues/Jean.yarn`:

```yarn
title: Jean_Start
tags: speaker:Jean
---
// Jean is standing outside his shop, visibly distressed
Jean: Катастрофа! Это конец! Мои колбаски... Мои драгоценные трюфельные колбаски!
Jean: Двадцать четыре штуки! Полгода работы! Всё пропало!

-> Что случилось, месье Жан?
    Jean: Кто-то ночью обчистил мою лавку! Все элитные колбаски с трюфелями — исчезли!
    Jean: А через неделю фестиваль! Я же готовил их специально на конкурс!
    -> А дверь? Следы взлома?
        Jean: В том-то и дело! Дверь цела, замок не тронут!
        Jean: Как будто вор прошёл сквозь стены! Или... через что-то другое.
    -> Полиция уже была?
        Jean: Ха! Этот Рампье? Он только и делает, что подозревает всех подряд!
        Jean: Он уже обвинил бродячих собак, соседей и даже МЕНЯ самого!

-> Кого вы подозреваете?
    Jean: Я не знаю... Но колбаски были уникальные — с трюфелями!
    Jean: Такие трюфели продаёт только одно место в городе — бутик Мадам Пушильды.
    Jean: Но она такая респектабельная дама... Хотя...
    <<set_flag jean_suspects_pushilda>>

-> Я помогу вам разобраться!
    Jean: Ты?! Пьер, ты же всего лишь кот!
    Jean: Хотя... У тебя нюх получше, чем у всей полиции города.
    Jean: Ладно! Если найдёшь мои колбаски — пожизненный запас обрезков! Обещаю!
    <<start_quest main_q1>>
    <<complete_objective main_q1 talk_to_jean>>
    <<set_flag talked_to_jean>>
    Jean: Начни с моей лавки — осмотри место преступления.
    Jean: Вот, дверь открою для тебя. Смотри всё внимательно!
===

title: Jean_AfterQuest
tags: speaker:Jean
---
// After player has started the quest, Jean says shorter lines
<<if has_flag("cloth_found")>>
    Jean: Ну как? Нашёл что-нибудь?
    Jean: Синяя ткань? Хм... У меня такой нет. И у Рампье тоже...
    Jean: Надо бы поспрашивать в городе!
<<else>>
    Jean: Осмотри лавку как следует, Пьер! Нюх тебе в помощь!
<<endif>>
===
```

## 2. Rampier's Dialogue

Create `Assets/_PawsUp/Data/Dialogues/Rampier.yarn`:

```yarn
title: Rampier_Start
tags: speaker:Rampier
---
Рампье: О нет, только не ты. Иди отсюда, кот!
Рампье: Я веду ОФИЦИАЛЬНОЕ расследование. Котам тут не место!

-> Я тоже расследую это дело.
    Рампье: Ты?! Расследуешь? Ха-ха-ха!
    Рампье: Ступай ловить мышей, это больше по твоей части.
    Рампье: А ну брысь! Кошмар!
    // Рампье зовёт бульдога, но тот не приходит
    Рампье: Кошмар?! Да где его носит...

-> У вас есть подозреваемые?
    Рампье: У меня ВСЕ подозреваемые! Это мой метод — подозревать всех!
    Рампье: *чешет затылок*
    Рампье: Но улик пока... маловато. Совсем нет, если честно.

-> До свидания.
    Рампье: Вот и правильно. И чтоб я тебя тут больше не видел!
    // Пьер всё равно продолжает расследование
===

title: Rampier_AfterClue
tags: speaker:Rampier
---
<<if has_item("clue_blue_cloth")>>
    Рампье: Что это у тебя? Синяя ткань?
    Рампье: Дай сюда! Это УЛИКА! Коты не должны трогать улики!
    -> Это я нашёл, не вы.
        Рампье: Грр... Ладно. Но официально — это нашёл Я!
    -> Может, поработаем вместе?
        Рампье: Работать с КОТОМ?! Ни за что!
        Рампье: ...Но если что-то ещё найдёшь — скажи. Негласно.
<<else>>
    Рампье: Ты всё ещё тут? Ступай уже!
<<endif>>
===
```

## 3. ConditionalDoor

Create `Assets/_PawsUp/Scripts/Interaction/ConditionalDoor.cs`:

```csharp
using UnityEngine;
using PawsUp.Quest;
using PawsUp.Dialogue;

namespace PawsUp.Interaction
{
    /// <summary>
    /// A door that only opens when a specific flag is set.
    /// Otherwise shows a message via PierreMonologue.
    /// </summary>
    public class ConditionalDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private string requiredFlag = "talked_to_jean";
        [SerializeField] private string lockedPrompt = "[E] Войти";
        [SerializeField] private string lockedMessage = "Дверь заперта. Нужно сначала поговорить с владельцем.";
        [SerializeField] private string unlockedPrompt = "[E] Войти в лавку";

        [Header("Scene Transition")]
        [SerializeField] private string targetScene;
        [SerializeField] private string targetSpawnId;

        public string GetInteractionPrompt()
        {
            bool unlocked = QuestManager.Instance != null && QuestManager.Instance.HasFlag(requiredFlag);
            return unlocked ? unlockedPrompt : lockedPrompt;
        }

        public bool CanInteract() => true;

        public void Interact()
        {
            bool unlocked = QuestManager.Instance != null && QuestManager.Instance.HasFlag(requiredFlag);

            if (!unlocked)
            {
                // Show locked message
                var monologue = FindAnyObjectByType<PierreMonologue>();
                if (monologue != null)
                    monologue.ShowThought(lockedMessage);
                else
                    Debug.Log($"[Door] Locked: {lockedMessage}");
                return;
            }

            // Complete the "enter shop" objective
            QuestManager.Instance?.CompleteObjective("main_q1", "enter_shop");

            // Trigger scene transition (will work after step 18)
            var transition = SceneManagement.SceneTransitionManager.Instance;
            if (transition != null)
                transition.LoadScene(targetScene, targetSpawnId);
            else
                Debug.Log($"[Door] → {targetScene} / {targetSpawnId}");
        }
    }
}
```

## 4. Pierre's Thought Lines

Create `Assets/_PawsUp/Data/Dialogues/PierreThoughts.yarn`:

```yarn
title: Pierre_SeesCrowd
---
// When Pierre looks out the attic window
Пьер: *принюхивается* Что за шум внизу? Жан орёт как резаный...
Пьер: Хм. Пахнет неприятностями. И колбасой.
Пьер: Надо бы спуститься и разобраться.
===

title: Pierre_EntersStreet
---
Пьер: Так-так... Жан стоит у лавки, весь красный. Рампье тут как тут.
Пьер: Подойду-ка поближе. У меня нюх на загадки. И на колбасу.
===
```

## 5. NPC Setup Instructions (for step 21)

In CentralStreet scene at Jean_Position and Rampier_Position:
1. Create capsule placeholder for Jean (blue color, taller)
   - Add `TalkableNPC` component: yarnNodeName = "Jean_Start", npcName = "Жан Колбасье"
   - After quest started, change to "Jean_AfterQuest" (via script or second TalkableNPC)
2. Create capsule for Rampier (gray, with hat-sphere on top)
   - Add `TalkableNPC`: yarnNodeName = "Rampier_Start", npcName = "Инспектор Рампье"
3. On JeanShopDoor:
   - Replace DoorTrigger with `ConditionalDoor`
   - requiredFlag: "talked_to_jean"
   - targetScene: "ButcherShop"
   - targetSpawnId: "from_street"
   - lockedMessage: "Лавка Жана. Дверь закрыта... Надо бы поговорить с хозяином."

## Verification
- Talk to Jean → 3 dialogue choices work
- Choose "Я помогу" → quest starts, flag set, objective completed
- Try shop door BEFORE talking → "Лавка Жана. Дверь закрыта..."
- Talk to Jean first, THEN try door → transitions (or logs target scene)
- Talk to Rampier → his grumpy dialogue plays
- After finding clue, Rampier has different dialogue (conditional)
