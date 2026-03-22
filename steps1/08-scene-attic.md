# Step 08 — Сцена: Чердак Пьера в Unity 6 [Человек]

## Контекст
Unity 6000.3.11f1. Создаём первую игровую сцену — чердак, где живёт Пьер.
Шаг 8 из 21. Зависит от: Шаги 01-07.

## Описание локации
Маленькая мансардная комната на чердаке дома напротив колбасной лавки Жана. Наклонный потолок, окно с видом на улицу, уютная атмосфера. Здесь Пьер спит и начинает своё расследование.

## Пошаговая инструкция

### 1. Создание сцены
1. File → New Scene → Basic (URP)
2. File → Save As → `Assets/_PawsUp/Scenes/PierresAttic.unity`
3. Удали дефолтные объекты кроме **Directional Light** и **Main Camera**

### 2. Установка ProBuilder (если ещё не установлен)
1. Window → Package Manager
2. В списке найди **ProBuilder** → Install
3. После установки: Tools → ProBuilder → ProBuilder Window (закрепи панель)

### 3. Пол
1. ProBuilder toolbar → New Shape → **Cube**
2. В диалоге: X=4, Y=0.1, Z=3
3. Position: (0, 0, 0)
4. Имя объекта в Hierarchy: `Floor`

### 4. Стены
Создай 4 стены (ProBuilder → New Shape → Cube для каждой):

| Имя | Размер (X,Y,Z) | Position | Rotation |
|-----|-----------------|----------|----------|
| Wall_Left | 0.15, 2.5, 3 | (-2.0, 1.25, 0) | — |
| Wall_Right | 0.15, 2.5, 3 | (2.0, 1.25, 0) | — |
| Wall_Back | 4, 2.5, 0.15 | (0, 1.25, -1.5) | — |
| Wall_Front | 4, 2.5, 0.15 | (0, 1.25, 1.5) | — |

### 5. Наклонный потолок (крыша)
1. ProBuilder → New Shape → Cube → X=4.2, Y=0.1, Z=3.2
2. Position: (0, 2.5, 0)
3. Rotation X: 15 (наклон — мансарда)
4. Имя: `Ceiling_Sloped`

### 6. Окно (в Wall_Front)
1. Выдели `Wall_Front`
2. ProBuilder → выдели грань (Face mode) на стене где хочешь окно (правая часть стены, на высоте ~1.2м)
3. С выделенной гранью: Extrude Face → Distance: -0.15 (вдавить внутрь) → Delete Face
4. Или проще: сделай вырез — удали прямоугольник из стены и добавь 2 тонких куба как раму
5. **Рама окна:** два Cube (горизонтальный + вертикальный), тонкие (0.03 толщина)
6. **Стекло:** Cube (X=0.8, Y=0.6, Z=0.02) с полупрозрачным материалом

### 7. Создание материала для стекла
1. В Project: ПКМ → Create → Material → назови `Glass_Window`
2. Shader: Universal Render Pipeline/Lit
3. Surface Type: **Transparent**
4. Base Map: голубоватый (#C8E0FF), Alpha: 0.2
5. Smoothness: 0.9
6. Назначь на объект стекла

### 8. Предметы интерьера

#### Кровать/лежанка
1. Cube (1.2×0.15×0.7) — основание, Position: (-1.0, 0.15, -0.8). Имя: `Bed_Base`
2. Cube (0.4×0.1×0.3) — подушка сверху. Имя: `Bed_Pillow`
3. Создай материал `Fabric_Bed`: тёплый коричневый (#8B6914), Roughness 0.85

#### Стопка газет
1. 3 тонких Cube (0.3×0.02×0.2), слегка повёрнутые, стопкой
2. Position: (1.0, 0.12, 0.5). Имя: `Newspapers`
3. Тег: `Interactable` (для будущего осмотра)

#### Рыбные кости
1. 2-3 маленьких Capsule (scale 0.03×0.15×0.03)
2. Разбросаны у кровати на полу
3. Имя: `FishBones`

#### Миска
1. Cylinder (radius 0.12, height 0.05)
2. Position рядом с кроватью, на полу
3. Имя: `FoodBowl`. Тег: `Interactable`

### 9. Лестница / люк вниз
1. В полу: создай проём — удали часть пола (ProBuilder: выдели face, Delete)
   - Или: два куба пола с промежутком 0.8м
2. Проём: Position (1.5, 0, -1.0), размер ~0.8×0.8
3. Добавь ступени: 3-4 Cube (0.8×0.05×0.25) с шагом по Y=-0.25, Z=0.25
4. Имя: `Stairs_Down`

### 10. Триггеры

#### Триггер у окна
1. Hierarchy → Create Empty → имя `WindowTrigger`
2. Position: у окна (приблизительно 1.5, 0.5, 1.2)
3. Add Component → Box Collider → Is Trigger ✅, Size: (1.0, 1.5, 0.5)
4. Тег: `Interactable` (для системы взаимодействия)

#### Триггер выхода (у лестницы)
1. Create Empty → имя `ExitPortal`
2. Position: у проёма в полу (1.5, 0.2, -1.0)
3. Box Collider → Is Trigger ✅, Size: (1.0, 0.5, 1.0)
4. (В шаге 21 сюда добавим ScenePortal компонент)

### 11. Точка появления
1. Create Empty → имя `SpawnPoint_default`
2. Position: (0, 0.1, 0) — центр комнаты
3. (В шаге 21 добавим компонент SpawnPoint)

### 12. Освещение

1. **Directional Light** (уже есть):
   - Rotation: (50, -30, 0) — утреннее солнце, падает через окно
   - Color: #FFE4B5 (тёплый жёлтый)
   - Intensity: 1.2
   - Shadow Type: Soft Shadows

2. **Point Light у окна:**
   - Hierarchy → Light → Point Light
   - Position: у окна (1.5, 1.5, 1.3)
   - Color: #FFF5E0 (тёплый)
   - Range: 4
   - Intensity: 0.8

3. **Ambient:**
   - Window → Rendering → Lighting
   - Environment → Source: Color
   - Ambient Color: #FFE8CC (тёплый оранжевый, слабый)
   - Fog: off

### 13. Материалы для стен и пола
1. `Wall_Plaster`: Base Color #E8D5B5, Roughness 0.9 — старая штукатурка
2. `Floor_Wood`: Base Color #A0784A, Roughness 0.7 — деревянные доски
3. `Ceiling_Wood`: Base Color #8B7355, Roughness 0.75 — тёмное дерево
4. Назначь на соответствующие объекты (перетащи материал на объект в Scene view)

### 14. Camera Preset Trigger
1. Create Empty → имя `CameraPresetTrigger_Indoor`
2. Position: (0, 1, 0) — центр комнаты
3. Box Collider → Is Trigger ✅, Size: (5, 3, 4) — покрывает всю комнату
4. (В шаге 21 добавим компонент CameraPresetTrigger, preset: Indoor)

### 15. Организация Hierarchy
Структурируй объекты:
```
PierresAttic (сцена)
  --- ENVIRONMENT ---
    Floor
    Wall_Left
    Wall_Right
    Wall_Back
    Wall_Front
    Ceiling_Sloped
    Window_Frame
    Window_Glass
    Stairs_Down
  --- PROPS ---
    Bed_Base
    Bed_Pillow
    Newspapers
    FishBones
    FoodBowl
  --- TRIGGERS ---
    WindowTrigger
    ExitPortal
    SpawnPoint_default
    CameraPresetTrigger_Indoor
  --- LIGHTING ---
    Directional Light
    PointLight_Window
```

### 16. Финальная проверка
- [ ] Пьер (префаб) помещается в комнату и не проваливается
- [ ] Камера переключается на Indoor пресет
- [ ] Все объекты имеют коллайдеры (Пьер не проходит сквозь стены)
- [ ] Окно полупрозрачное
- [ ] Освещение тёплое, уютное
- [ ] Триггеры видны в Scene view (зелёные рамки)
- Ctrl+S — сохрани сцену!
