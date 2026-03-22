# Step 21 — Финальная сборка и сквозной тест [Человек]

## Контекст
Unity 6000.3.11f1. Финальный шаг — подключение всех компонентов и тестирование.
Шаг 21 из 21. Зависит от: ВСЕ предыдущие шаги (01-20).

## Часть 1: Build Settings

1. File → Build Settings
2. Добавь сцены в этом порядке (перетаскивай из Project в список):
   - `Assets/_PawsUp/Scenes/Bootstrap.unity` — index 0
   - `Assets/_PawsUp/Scenes/PierresAttic.unity` — index 1
   - `Assets/_PawsUp/Scenes/CentralStreet.unity` — index 2
   - `Assets/_PawsUp/Scenes/ButcherShop.unity` — index 3
3. Закрой Build Settings

## Часть 2: Bootstrap сцена

Открой `Bootstrap.unity`:

### GameObject: `--- MANAGERS ---`
Убедись, что на нём есть компоненты:
- `GameManager`
- `InventoryManager` → в allQuests: перетащи SO `main_q1`
- `QuestManager` → в allQuests: перетащи SO `main_q1`
- `DialogueManager` → (см. Часть 2.1)
- `SceneTransitionManager` → fadeCanvasGroup: (см. Часть 2.2)
- `AudioManager` → (см. Часть 2.3)
- `SFXLibrary` → назначь клипы из `Assets/SourceFiles/SoundFX/`
- `HUDManager` → (см. Часть 2.4)
- `BootstrapLoader` → firstScene: "PierresAttic", firstSpawnId: "default"

### 2.1 Dialogue Runner (Yarn Spinner)
1. На `--- MANAGERS ---`: Add Component → **Dialogue Runner**
2. Create → Yarn Spinner → **Yarn Project** → сохрани в `_PawsUp/Data/Dialogues/`
3. В Yarn Project: добавь все .yarn файлы (Jean.yarn, Rampier.yarn, Investigation_Shop.yarn, PierreThoughts.yarn, Test.yarn)
4. На Dialogue Runner: назначь Yarn Project
5. Add Component → **In Memory Variable Storage**
6. Add Component → **YarnCustomCommands**
7. В DialogueManager: назначь Dialogue Runner

### 2.2 Fade Canvas
1. На `--- MANAGERS ---` создай child: **Canvas** (Screen Space - Overlay)
   - Sort Order: 100 (поверх всего)
2. Child → **Image** → цвет: чёрный (0,0,0,1), Stretch (Alt+клик bottom-right якорь)
3. На Image: Add Component → **Canvas Group** → Alpha: 0, Blocks Raycasts: off
4. В SceneTransitionManager → fadeCanvasGroup: перетащи этот Canvas Group

### 2.3 Audio Sources
1. На `--- MANAGERS ---` создай 3 child AudioSource:
   - `BGM_A`: Loop ✅, Play On Awake ❌, Volume 0
   - `BGM_B`: Loop ✅, Play On Awake ❌, Volume 0
   - `SFX`: Loop ❌, Play On Awake ❌
2. В AudioManager: назначь bgmSourceA, bgmSourceB, sfxSource

### 2.4 HUD Canvas
1. На `--- MANAGERS ---` создай child: **Canvas** "HUD_Canvas"
   - Screen Space - Overlay, Sort Order: 10
2. **Smell Bar** (top-left):
   - Panel (200×30px) → child Image "Fill" (тип Image: Filled, Fill Method: Horizontal)
   - Add Component → `SmellEnergyBar` → fillImage: назначь Fill Image
3. **Quest Tracker** (top-right):
   - Panel → 2 TextMeshPro: "QuestTitle" (Bold, 18px) и "Objective" (14px)
   - Add Component → `QuestTrackerUI` → назначь тексты
4. **Interaction Prompt** (bottom-center):
   - Panel → TextMeshPro "[E] Действие"
   - Add Component → `InteractionPromptUI` → назначь canvas и text
5. **Inventory Panel** (center, SetActive false):
   - Фон: Image (чёрный, alpha 0.8), Stretch
   - Grid: Panel с Grid Layout Group (Cell Size 80×80, Columns 4)
   - Details: Panel справа (Image + 2 TMPro)
   - Add Component → `InventoryUI` → назначь всё
   - Создай Slot Prefab: Image (80×80) + Button + child Image "Icon" + child TMPro "Count"
6. **Pause Panel** (center, SetActive false):
   - Фон: Image (тёмный)
   - TMPro "ПАУЗА"
   - Button "Продолжить" → OnClick → PauseMenuUI.OnResumeClick
   - Button "Выход" → OnClick → PauseMenuUI.OnQuitClick
   - Add Component → `PauseMenuUI`
7. **Pierre Monologue** (lower-third):
   - Panel (полупрозрачный тёмный фон, снизу, высота ~100px, SetActive false)
   - TMPro (italic, светлый жёлтый цвет, 16px)
   - Add Component → `PierreMonologue` → назначь panel и text
8. **Dialogue Panel** (bottom):
   - Настрой Yarn Spinner Line View и Options List View
   - Панель с портретом (Image 100×100), именем (TMPro Bold), текстом (TMPro)
   - Вертикальный список кнопок для выборов

## Часть 3: Сцена PierresAttic

Открой `PierresAttic.unity`:

1. **Pierre:** перетащи префаб Pierre → Position на кровати (-1.0, 0.4, -0.8)
   - Убедись: tag = `Player`
   - Add Component → `PersistentPlayer`
   - Add Component → `InteractionDetector`
   - Add Component → `FootstepSystem` (назначь клипы шагов)
   - SmellSense: Add Component → `SmellSenseManager` (или на отдельный child)
     - smellVolume: создай Global Volume → SmellSense Profile (см. шаг 14)
2. **Camera Rig:** создай CinemachineCamera'ы (см. шаг 07)
   - Add Component → `CameraManager` → назначь 3 камеры
3. **SpawnPoint_default:** Add Component → `SpawnPoint` → spawnId: "default"
4. **ExitPortal:** Add Component → `ScenePortal`
   - targetScene: "CentralStreet", targetSpawnId: "from_attic", useTrigger: true
5. **CameraPresetTrigger_Indoor:** Add Component → `CameraPresetTrigger`
   - enterPreset: Indoor
6. **SceneAudioLoader:** Create Empty → Add `SceneAudioLoader` → SceneConfig: PierresAttic_Config
7. **Осматриваемые объекты:**
   - Newspapers → Add `ExaminableYarnBridge` → yarnNodeName: нет (просто ExaminableObject с текстом "Старые газеты. «Сен-Сосиссон Газетт»")
   - FoodBowl → ExaminableObject → "Моя миска. Пустая, как обычно по утрам..."
   - WindowTrigger → ExaminableYarnBridge → yarnNodeName: "Pierre_SeesCrowd"

## Часть 4: Сцена CentralStreet

Открой `CentralStreet.unity`:

1. **Pierre:** НЕ размещай! Он придёт из другой сцены (DontDestroyOnLoad)
2. **SpawnPoints:**
   - SpawnPoint_from_attic → `SpawnPoint` → spawnId: "from_attic"
   - SpawnPoint_from_shop → `SpawnPoint` → spawnId: "from_shop"
3. **NPC Жан:**
   - Create → Capsule → Position: у двери лавки → цвет синий
   - Add `TalkableNPC` → yarnNodeName: "Jean_Start", npcName: "Жан Колбасье"
4. **NPC Рампье:**
   - Create → Capsule → Position: у фонтана → цвет серый
   - Add `TalkableNPC` → yarnNodeName: "Rampier_Start", npcName: "Инспектор Рампье"
5. **JeanShopDoor:** Remove DoorTrigger (если есть), Add `ConditionalDoor`
   - requiredFlag: "talked_to_jean"
   - targetScene: "ButcherShop", targetSpawnId: "from_street"
   - lockedMessage: "Лавка Жана. Закрыта... Нужно поговорить с хозяином."
6. **SceneAudioLoader** → CentralStreet_Config
7. **CameraPresetTrigger** → enterPreset: Exploration (на большом триггере, покрывающем всю улицу)

## Часть 5: Сцена ButcherShop

Открой `ButcherShop.unity`:

1. **SpawnPoint_from_street** → `SpawnPoint` → spawnId: "from_street"
2. **ExitPortal_to_street** → `ScenePortal`
   - targetScene: "CentralStreet", targetSpawnId: "from_shop", useTrigger: false (E to exit)
   - prompt: "[E] Выйти на улицу"
3. **DisplayCase** → `ExaminableYarnBridge` → yarnNodeName: "Pierre_Examine_DisplayCase"
4. **CashRegister** → `ExaminableYarnBridge` → yarnNodeName: "Pierre_Examine_CashRegister"
5. **OrderBook** → `ExaminableYarnBridge` → yarnNodeName: "Pierre_Examine_OrderBook"
6. **BackDoor** → `ExaminableYarnBridge` → yarnNodeName: "Pierre_Examine_BackDoor"
7. **BackDoorGap:**
   - Add `PickupObject` → itemData: clue_blue_cloth SO, requiresSmellSense: true
   - Add `SmellHighlight` → highlightColor: yellow, hideWhenInactive: true
   - Child BlueCloth_Visual → начально inactive (SmellHighlight управляет)
   - OnPickedUp event → DialogueManager.StartDialogue("Pierre_Found_BlueCloths")
8. **ScentTrail:** Create Empty → child ParticleSystem (из ScentTrailParticles)
   - Add `ScentTrail` → waypoints от DisplayCase к BackDoorGap
9. **InvestigationController:** Create Empty →
   - backDoorGapClue: BackDoorGap
   - scentTrailObject: ScentTrail
10. **SmellSense Volume:** Create → Volume → Global Volume → Profile: SmellSenseProfile
11. **SceneAudioLoader** → ButcherShop_Config
12. **CameraPresetTrigger** → enterPreset: Indoor

## Часть 6: Сквозной тест

Нажми Play (в сцене Bootstrap!).

### Чек-лист:
1. ✅ Bootstrap → фейд → загружается чердак
2. ✅ Пьер на кровати, камера Indoor (ближе)
3. ✅ WASD — Пьер ходит, анимации (или капсула двигается)
4. ✅ Мышь — камера вращается
5. ✅ Подойти к окну → [E] → диалог "Что за шум..."
6. ✅ Подойти к лестнице → автоматический переход (или [E])
7. ✅ Фейд → улица загружается, Пьер у SpawnPoint
8. ✅ Камера → Exploration (дальше)
9. ✅ Музыка сменилась (кроссфейд)
10. ✅ Подойти к Рампье → [E] → диалог, он ворчит
11. ✅ Подойти к двери лавки → [E] → "Закрыта, поговори с хозяином"
12. ✅ Подойти к Жану → [E] → диалог, выбрать "Я помогу"
13. ✅ HUD: квест "Расследовать кражу" появился, цель "Войти в лавку"
14. ✅ Дверь лавки → [E] → фейд → внутри лавки
15. ✅ Цель обновилась → "Использовать кошачий нюх"
16. ✅ Осмотреть витрину [E] → диалог Пьера
17. ✅ Осмотреть кассу, книгу, дверь → комментарии
18. ✅ Нажать Q → мир обесцвечивается → полоска нюха убывает
19. ✅ Цель → "Найти улику"
20. ✅ Следы видны (частицы от витрины к щели)
21. ✅ Щель светится жёлтым
22. ✅ [E] на щели → подбираем клочок ткани → анимация подбора
23. ✅ Диалог: "Что это?! Клочок синей ткани!"
24. ✅ Tab → инвентарь → "Клочок синей ткани" в ячейке
25. ✅ Цель → "Исследовать город"
26. ✅ [E] у двери → выход → улица
27. ✅ Esc → пауза → продолжить

### Если что-то не работает:
- **Пьер не появляется после перехода:** проверь PersistentPlayer и tag Player
- **Диалоги не запускаются:** проверь Yarn Project, все .yarn файлы добавлены
- **Нюх не работает:** проверь SmellSenseVolume в сцене, назначен ли в SmellSenseManager
- **Квест не обновляется:** проверь QuestManager.allQuests содержит SO
- **Фейд не работает:** проверь fadeCanvasGroup в SceneTransitionManager
- **Звука нет:** проверь AudioSources в AudioManager, SceneConfig заполнен

## Готово!
Первая локация полностью играбельна. Следующий батч `steps2/` — Мадам Пушильда, переулок, канализация, Крыс Рене, стелс с Кошмаром.
