# Step 06 — Импорт Пьера в Unity 6 [Человек]

## Контекст
Unity 6000.3.11f1. Импортируем FBX модель из Blender (шаг 05) и настраиваем.
Шаг 6 из 21. Зависит от: Шаги 01-03 (проект), 04-05 (модель).

## 1. Импорт FBX файла

1. Открой Unity проект
2. В окне **Project** перейди в `Assets/_PawsUp/Models/`
3. Перетащи файл `Pierre.fbx` из Finder прямо в эту папку
4. Unity начнёт импорт — подожди пока прогресс-бар завершится

## 2. Настройка Model Import Settings

Выдели `Pierre.fbx` в Project → в **Inspector** появятся вкладки:

### Вкладка Model
- **Scale Factor:** `1` (если модель правильного размера в Blender). Если Пьер слишком большой/маленький — подбери значение так, чтобы высота была ~0.8 метра
- **Convert Units:** ✅
- **Import BlendShapes:** ✅ (если есть Shape Keys)
- **Import Visibility:** ✅
- **Import Cameras:** ❌
- **Import Lights:** ❌
- **Meshes:**
  - Mesh Compression: Off
  - Read/Write: ✅ (нужно для будущих модификаций)
  - Generate Normals: Import
- Нажми **Apply** внизу Inspector

### Вкладка Rig
- **Animation Type:** `Generic` (НЕ Humanoid — Пьер это кот на четырёх лапах)
- **Avatar Definition:** `Create From This Model`
- **Root Node:** `Root` (или `Armature`, зависит от экспорта — выбери корневой объект арматуры)
- Нажми **Apply**

### Вкладка Animation
Здесь ты увидишь список анимационных клипов. Для каждого:

| Клип | Loop Time | Loop Pose |
|------|-----------|-----------|
| Idle | ✅ | ✅ |
| Walk | ✅ | ✅ |
| Run | ✅ | ✅ |
| Crouch_Walk | ✅ | ✅ |
| Sniff | ❌ | ❌ |
| PickUp | ❌ | ❌ |
| Jump | ❌ | ❌ |

Для каждого клипа:
1. Выбери его в списке внизу
2. В секции ниже:
   - **Loop Time:** поставь/убери галочку по таблице выше
   - **Loop Pose:** для зацикленных — поставь
   - **Root Transform Rotation:** Bake Into Pose ✅, Based Upon: Original
   - **Root Transform Position (Y):** Bake Into Pose ✅, Based Upon: Original
   - **Root Transform Position (XZ):** Bake Into Pose ✅, Based Upon: Original
3. Нажми **Apply**

### Вкладка Materials
1. **Location:** Use External Materials
2. Нажми **Extract Materials...** → выбери папку `Assets/_PawsUp/Materials/Pierre/` (создай если нет)
3. Нажми **Apply**
4. Перейди в `_PawsUp/Materials/Pierre/` — там появятся материалы
5. Для каждого материала:
   - Выдели его → в Inspector проверь что Shader = **Universal Render Pipeline/Lit**
   - Если нет — смени вручную: Shader dropdown → Universal Render Pipeline → Lit
   - Поправь цвета если нужно (Base Map — основной цвет)

## 3. Создание Animator Controller

1. Перейди в `Assets/_PawsUp/Animations/`
2. ПКМ → Create → **Animator Controller** → назови `Pierre_AnimatorController`
3. Двойной клик чтобы открыть окно **Animator**

### Параметры (слева внизу в окне Animator, вкладка Parameters):
Нажми `+` для каждого:
- `Speed` — **Float** (0 = стоит, 0.5 = идёт, 1 = бежит)
- `Grounded` — **Bool**
- `IsCrouching` — **Bool**
- `IsSniffing` — **Bool**
- `Jump` — **Trigger**

### Создание Blend Tree для передвижения
1. ПКМ в пустом пространстве Animator → Create State → From New Blend Tree
2. Назови `Locomotion`
3. Двойной клик на `Locomotion` → откроется Blend Tree
4. Тип: **1D**, Parameter: `Speed`
5. Добавь 3 motion fields (кнопка `+` → Add Motion Field):
   - Threshold 0: перетащи клип `Idle` из Project
   - Threshold 0.5: перетащи клип `Walk`
   - Threshold 1.0: перетащи клип `Run`
6. Кнопка ← (назад) чтобы вернуться к основному виду

### Создание Blend Tree для крадения
1. ПКМ → Create State → From New Blend Tree → назови `Crouch`
2. Двойной клик → 1D, Parameter: `Speed`
3. Два поля:
   - Threshold 0: `Idle` (или сделай отдельный Crouch_Idle если есть)
   - Threshold 0.5: `Crouch_Walk`

### Остальные состояния
1. ПКМ → Create State → Empty → назови `Sniff` → в Inspector: Motion = клип `Sniff`
2. ПКМ → Create State → Empty → назови `PickUp` → Motion = `PickUp`
3. ПКМ → Create State → Empty → назови `Jump` → Motion = `Jump`

### Переходы (Transitions)
ПКМ на состоянии → Make Transition → тяни стрелку к целевому состоянию.

| Откуда | Куда | Условие | Has Exit Time |
|--------|------|---------|---------------|
| Entry | Locomotion | (по умолчанию) | — |
| Locomotion | Crouch | IsCrouching = true | ❌ |
| Crouch | Locomotion | IsCrouching = false | ❌ |
| Any State | Sniff | IsSniffing = true | ❌ |
| Sniff | Locomotion | IsSniffing = false | ✅ (дождись конца анимации) |
| Any State | Jump | Jump (trigger) | ❌ |
| Jump | Locomotion | — | ✅ (Has Exit Time) |
| Any State | PickUp | (будет вызываться из кода через trigger) | ❌ |
| PickUp | Locomotion | — | ✅ |

Для каждого перехода выбери стрелку и в Inspector:
- **Has Exit Time:** по таблице
- **Transition Duration:** 0.1 (быстрый переход)
- **Conditions:** добавь условия по таблице

## 4. Создание префаба Пьера

1. Перетащи `Pierre` модель из `_PawsUp/Models/` в сцену (Hierarchy)
2. На корневом объекте Pierre в Inspector:
   - **Add Component:** `Character Controller`
     - Center: (0, 0.4, 0)
     - Radius: 0.3
     - Height: 0.8
   - **Add Component:** `PierreController` (из `PawsUp.Player`)
   - **Add Component:** `PawsUpInputs` (из `PawsUp.Core`)
   - **Add Component:** `Player Input`
     - Actions: перетащи `PawsUpActions` из `_PawsUp/Data/`
     - Default Map: Player
     - Behavior: Send Messages (или Invoke Unity Events)
3. Компонент **Animator** (уже на объекте):
   - Controller: перетащи `Pierre_AnimatorController`
   - Avatar: должен быть уже назначен (Pierre Avatar)
4. В PierreController:
   - Ground Layers: ✅ Default (или создай отдельный слой Ground)
5. Перетащи Pierre из Hierarchy в `Assets/_PawsUp/Prefabs/Player/` → создастся **префаб**
6. Удали Pierre из сцены (он теперь префаб, будем размещать в конкретных сценах)

## 5. Тестирование

1. Открой сцену `PierresAttic` (или любую тестовую)
2. Перетащи префаб Pierre в сцену
3. Добавь Plane (пол) если нет
4. Play → проверь:
   - [ ] WASD — Пьер ходит, анимация Walk играет
   - [ ] Shift — бежит, анимация Run
   - [ ] Ctrl — крадётся, анимация Crouch_Walk
   - [ ] Space — прыжок, анимация Jump
   - [ ] Модель не проваливается сквозь пол
   - [ ] Поворачивается плавно в направлении движения
   - [ ] Нет ошибок в Console
