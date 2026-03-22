# Step 09 — Сцена: Центральная улица в Unity 6 [Человек]

## Контекст
Unity 6000.3.11f1. Главная улица городка Сен-Сосиссон.
Шаг 9 из 21. Зависит от: Шаг 07 (камера).

## Описание локации
Мощёная улица французского городка. По обе стороны — фасады домов. Лавка Жана Колбасье с дверью. Фонтан в центре. Переулок, ведущий к канализации (для будущих шагов). Здесь Пьер встречает Жана и инспектора Рампье.

## Пошаговая инструкция

### 1. Создание сцены
1. File → New Scene → Basic (URP)
2. Save As → `Assets/_PawsUp/Scenes/CentralStreet.unity`
3. Удали дефолтные объекты кроме Directional Light и Main Camera

### 2. Земля (мостовая)
1. ProBuilder → New Shape → Cube → X=40, Y=0.2, Z=20
2. Position: (0, -0.1, 0)
3. Имя: `Ground`
4. Материал: `Cobblestone` — Base Color #9B8B7A, Roughness 0.85

### 3. Фасады зданий (левая сторона)
Расставь кубы вдоль левого края улицы (X отрицательный):

| Имя | Размер (X,Y,Z) | Position | Цвет материала |
|-----|-----------------|----------|----------------|
| Building_L1 | 6, 5, 1.5 | (-7, 2.5, -9) | #E8D5A8 (бежевый) |
| Building_L2 | 5, 6, 1.5 | (-7, 3, -3) | #D4C4A0 (песочный) |
| Building_L3_JeanShop | 7, 4.5, 1.5 | (-7, 2.25, 4) | #C8B898 (тёплый серый) |
| Building_L4 | 5, 5.5, 1.5 | (-7, 2.75, 9) | #DDD0B8 (кремовый) |

### 4. Фасады зданий (правая сторона)

| Имя | Размер (X,Y,Z) | Position | Цвет |
|-----|-----------------|----------|------|
| Building_R1 | 6, 6, 1.5 | (7, 3, -8) | #E0D0B0 |
| Building_R2 | 5, 4, 1.5 | (7, 2, -2) | #D8C8A8 |
| Building_R3 | 7, 5, 1.5 | (7, 2.5, 5) | #E8D8B8 |
| Building_R4 | 4, 5.5, 1.5 | (7, 2.75, 10) | #D0C0A0 |

### 5. Крыши
Для каждого здания добавь крышу:
1. ProBuilder → Cube → размер по X,Z здания, Y=0.3
2. Rotation X: 15° (наклон)
3. Position: над зданием
4. Материал: `Roof_Tiles` — Base Color #8B4513 (черепичный коричневый), Roughness 0.75

### 6. Дверь лавки Жана
1. На Building_L3_JeanShop — создай дверной проём:
   - Cube для двери: X=1.2, Y=2.2, Z=0.1
   - Position: на фасаде лавки, уровень пола
   - Материал: `Door_Wood` — тёмное дерево #5C3A1E
2. Имя: `JeanShopDoor`
3. Add Component → Box Collider → Is Trigger ✅
4. Тег: `Interactable`

### 7. Вывеска лавки
1. Cube: X=2, Y=0.4, Z=0.05
2. Position: над дверью лавки, на фасаде
3. Материал: `Sign_Jean` — Base Color #8B0000 (тёмно-красный)
4. Имя: `Sign_ButcherShop`
5. (Текст добавим позже через TextMeshPro 3D или текстуру)

### 8. Фонтан
1. **Чаша:** Cylinder — Radius 1.5, Height 0.5, Position: (0, 0.25, 0)
2. **Столб:** Cylinder — Radius 0.2, Height 1.2, Position: (0, 0.85, 0)
3. **Верхняя чаша:** Cylinder — Radius 0.5, Height 0.15, Position: (0, 1.5, 0)
4. Объедини в пустой GO `Fountain`
5. Материал: `Stone_Fountain` — светло-серый #C0C0C0, Roughness 0.7
6. Position фонтана: (0, 0, -3) — ближе к центру улицы

### 9. Переулок (к будущей канализации)
1. Два параллельных куба-стены:
   - Cube: X=0.3, Y=3, Z=6 — Position: (3, 1.5, -9.5)
   - Cube: X=0.3, Y=3, Z=6 — Position: (5, 1.5, -9.5)
2. Пространство между ними = переулок (ширина ~1.7м)
3. В конце переулка — тупик (куб-стена) — для будущего входа в канализацию
4. Имя группы: `Alley`

### 10. Уличная мебель
- **Скамейка:** Cube 1.5×0.5×0.4, Position: (2, 0.25, 2). `Bench`
- **Фонарь:** Cylinder 0.1×3×0.1 + Sphere 0.15 наверху, Position: (-3, 1.5, 0). `StreetLamp`
  - Point Light внутри сферы: Color #FFE0A0, Range 5, Intensity 0.5
- **Бочки:** Cylinder 0.3×0.6×0.3 × 2-3 шт. у стен. `Barrel_1`, `Barrel_2`
- **Ящики:** Cube 0.5×0.5×0.5, у стен. `Crate_1` (для будущих укрытий стелса)

### 11. Позиции NPC
Создай пустые GameObject'ы (без меша, только метки):
1. `Jean_Position` — Position: (-5.5, 0, 4) — у двери лавки
2. `Rampier_Position` — Position: (1, 0, -2) — у фонтана
3. (В шаге 21 тут будут NPC)

### 12. Точки появления (SpawnPoints)
1. `SpawnPoint_from_attic` — Position: (5, 0, 6) — Пьер приходит "с чердака"
2. `SpawnPoint_from_shop` — Position: (-5, 0, 4) — выход из лавки

### 13. Порталы (выходы)
1. `ExitPortal_to_shop` — у двери лавки, Box Collider isTrigger
   - (будет ScenePortal → ButcherShop / from_street)

### 14. Освещение
1. **Directional Light:**
   - Rotation: (35, -60, 0) — утреннее солнце, косые лучи
   - Color: #FFF5E0 (тёплый)
   - Intensity: 1.5
   - Shadows: Soft Shadows, Strength 0.7

2. **Ambient:**
   - Window → Rendering → Lighting
   - Environment Source: Gradient
   - Sky Color: #87CEEB (голубое небо)
   - Equator: #FFF8DC
   - Ground: #8B8B7A

### 15. NavMesh (для будущих NPC)
1. Window → AI → Navigation
2. Вкладка **Agents:** Agent Radius 0.3, Agent Height 0.8 (размер кота)
3. Вкладка **Bake:**
   - Agent Radius: 0.3
   - Agent Height: 0.8
   - Max Slope: 45
   - Step Height: 0.3
4. Нажми **Bake**
5. Зелёная/голубая сетка должна покрыть пол улицы (не здания)

### 16. Skybox
1. Window → Rendering → Lighting → Environment
2. Skybox Material: можно использовать дефолтный или создать:
   - Create → Material → Shader: Skybox/Procedural
   - Sun Size: 0.04, Atmosphere Thickness: 1.0
   - Ground Color: #BEB89C

### 17. Организация Hierarchy
```
CentralStreet
  --- ENVIRONMENT ---
    Ground
    Buildings_Left/
      Building_L1..L4
    Buildings_Right/
      Building_R1..R4
    Roofs/
    Alley/
  --- PROPS ---
    Fountain
    JeanShopDoor
    Sign_ButcherShop
    Bench
    StreetLamp
    Barrel_1, Barrel_2
    Crate_1
  --- NPC_POSITIONS ---
    Jean_Position
    Rampier_Position
  --- TRIGGERS ---
    SpawnPoint_from_attic
    SpawnPoint_from_shop
    ExitPortal_to_shop
  --- LIGHTING ---
    Directional Light
    StreetLamp_Light
```

### 18. Финальная проверка
- [ ] Пьер ходит по улице и не проваливается
- [ ] Все здания имеют коллайдеры (стены блокируют)
- [ ] Дверь лавки — триггер
- [ ] NavMesh покрывает проходимую область
- [ ] Освещение тёплое, "французское утро"
- [ ] Камера переключается на Exploration пресет
- Ctrl+S!
