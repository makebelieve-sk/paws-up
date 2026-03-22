# Step 10 — Сцена: Лавка Жана в Unity 6 [Человек]

## Контекст
Unity 6000.3.11f1. Интерьер колбасной лавки — место преступления.
Шаг 10 из 21. Зависит от: Шаг 07 (камера).

## Описание локации
Лавка мясника Жана Колбасье. Прилавок с пустой витриной (колбасы украдены!), кассовый аппарат, книга заказов, полки с оставшимися товарами, задняя дверь (заперта), щель под задней дверью (здесь улика — клочок синей ткани). Атмосфера — тепло, но тревожно.

## Пошаговая инструкция

### 1. Создание сцены
1. File → New Scene → Basic (URP)
2. Save As → `Assets/_PawsUp/Scenes/ButcherShop.unity`
3. Удали дефолтные объекты кроме Directional Light, Main Camera

### 2. Комната (стены, пол, потолок)

| Объект | Размер (X,Y,Z) | Position | Имя |
|--------|-----------------|----------|-----|
| Пол | 8, 0.15, 6 | (0, 0, 0) | Floor |
| Потолок | 8, 0.1, 6 | (0, 3, 0) | Ceiling |
| Стена левая | 0.15, 3, 6 | (-4, 1.5, 0) | Wall_Left |
| Стена правая | 0.15, 3, 6 | (4, 1.5, 0) | Wall_Right |
| Стена задняя | 8, 3, 0.15 | (0, 1.5, -3) | Wall_Back |
| Стена передняя | 8, 3, 0.15 | (0, 1.5, 3) | Wall_Front |

Материалы:
- Пол: `Shop_Floor_Tiles` — #B8A88A, Roughness 0.8 (плитка)
- Стены: `Shop_Wall` — #F5ECD7, Roughness 0.9 (светлая штукатурка)
- Потолок: `Shop_Ceiling` — #FFFFFF, Roughness 0.95

### 3. Входная дверь (в Wall_Front)
1. Вырез в стене: ProBuilder → выдели face → Extrude -0.15 → Delete face
2. Или: два куба стены с промежутком 1.2м
3. Дверная рама: тонкие кубы (#5C3A1E дерево)
4. Положение: по центру стены Front или чуть правее

### 4. Прилавок
1. Cube: X=3, Y=1.0, Z=0.8
2. Position: (0, 0.5, 0.5) — ближе к входу
3. Материал: `Counter_Wood` — #6B4423, Roughness 0.7
4. Имя: `Counter`

### 5. Витрина (пустая — колбасы украдены!)
1. Cube: X=2.5, Y=0.8, Z=0.6
2. Position: на прилавке или рядом с ним (-0.5, 0.5, 1.5)
3. Создай материал `Glass_Display`:
   - Shader: URP/Lit
   - Surface Type: **Transparent**
   - Base Map: #F0F8FF, Alpha: 0.15
   - Smoothness: 0.95
4. Имя: `DisplayCase`
5. **Тег: `Interactable`**
6. Add Component → Box Collider (обычный, не trigger — для визуального обнаружения)

### 6. Кассовый аппарат
1. Cube: X=0.3, Y=0.25, Z=0.3
2. Position: на прилавке, справа (1.0, 1.1, 0.5)
3. Материал: `Metal_Dark` — #3A3A3A, Metallic 0.6, Roughness 0.4
4. Имя: `CashRegister`
5. **Тег: `Interactable`**

### 7. Книга заказов
1. Cube: X=0.25, Y=0.03, Z=0.35 (плоская книга)
2. Position: на прилавке, слева (-0.8, 1.03, 0.5)
3. Материал: `Book_Leather` — #4A2A0A, Roughness 0.85
4. Имя: `OrderBook`
5. **Тег: `Interactable`**

### 8. Полки на стенах
3 полки на левой и/или задней стене:
1. Cube: X=2, Y=0.08, Z=0.3 × 3 штуки
2. Позиции по Y: 1.2, 1.7, 2.2 (одна над другой)
3. На полках — маленькие кубы-заглушки (оставшиеся товары)
4. Имя: `Shelf_1`, `Shelf_2`, `Shelf_3`

### 9. Задняя дверь
1. Cube: X=1.0, Y=2.2, Z=0.1
2. Position: в Wall_Back, чуть правее центра (1.5, 1.1, -2.9)
3. Материал: `Door_Wood_Old` — #4A3520, Roughness 0.8
4. Имя: `BackDoor`
5. **Тег: `Interactable`**

### 10. Щель под задней дверью (ГЛАВНАЯ УЛИКА!)
Это самый важный интерактивный объект — здесь Пьер найдёт клочок синей ткани.

1. Create Empty → имя `BackDoorGap`
2. Position: прямо под задней дверью (1.5, 0.03, -2.85)
3. Add Component → Box Collider → Size: (0.8, 0.06, 0.15), Is Trigger: ✅
4. **Тег: `Interactable`**
5. Маленький визуальный объект (синий клочок ткани):
   - Child Cube: X=0.08, Y=0.01, Z=0.06
   - Материал: `ClueBlueCloth` — Base Color #2244AA (синий), Roughness 0.75
   - **По умолчанию выключен** (SetActive false) — станет видим только при кошачьем нюхе
6. Этот объект получит компоненты SmellHighlight + PickupObject в шаге 21

### 11. Дополнительные детали
- **Весы:** маленький набор из кубов на прилавке (0.15×0.1×0.1)
- **Разделочная доска:** плоский Cube на прилавке
- **Крючки на стене:** маленькие цилиндры (для подвешивания колбас — пустые)
- **Фартук:** плоскость на стене (деталь атмосферы)

### 12. Освещение
1. **Directional Light:**
   - Отключи или сделай Intensity 0.3 (внутри помещения — мало прямого солнца)

2. **Point Light 1 (основной):**
   - Position: (0, 2.7, 0) — потолочный
   - Color: #FFF0D0 (тёплый)
   - Range: 8
   - Intensity: 1.5

3. **Point Light 2 (витрина):**
   - Position: (-0.5, 1.8, 1.5) — над витриной
   - Color: #FFFFFF
   - Range: 3
   - Intensity: 0.8

4. **Ambient:** Window → Rendering → Lighting
   - Environment Source: Color
   - Ambient Color: #FFE8CC, Intensity 0.4

### 13. Точка появления
1. Create Empty → `SpawnPoint_from_street`
2. Position: (0, 0.15, 2.5) — у входной двери

### 14. Выходной портал
1. Create Empty → `ExitPortal_to_street`
2. Position: (0, 1, 2.9) — у входной двери
3. Box Collider → Is Trigger ✅, Size: (1.5, 2.2, 0.3)

### 15. Camera Preset Trigger
1. Create Empty → `CameraPresetTrigger_Indoor`
2. Position: (0, 1.5, 0)
3. Box Collider → Is Trigger ✅, Size: (9, 4, 7) — покрывает всю комнату

### 16. Организация Hierarchy
```
ButcherShop
  --- ENVIRONMENT ---
    Floor
    Ceiling
    Wall_Left / Right / Back / Front
    Door_Frame
  --- PROPS ---
    Counter
    DisplayCase        [Interactable]
    CashRegister       [Interactable]
    OrderBook          [Interactable]
    BackDoor           [Interactable]
    BackDoorGap        [Interactable] ← УЛИКА
      BlueCloth_Visual (inactive)
    Shelf_1, Shelf_2, Shelf_3
    ShopDetails/
  --- TRIGGERS ---
    SpawnPoint_from_street
    ExitPortal_to_street
    CameraPresetTrigger_Indoor
  --- LIGHTING ---
    PointLight_Ceiling
    PointLight_Display
```

### 17. Финальная проверка
- [ ] Пьер ходит по лавке, не проваливается
- [ ] Все 5 интерактивных объектов имеют тег `Interactable` и коллайдеры
- [ ] BackDoorGap — триггер, визуал синей ткани выключен
- [ ] Камера → Indoor пресет (ближе)
- [ ] Освещение тёплое, интерьерное
- [ ] Есть SpawnPoint и ExitPortal
- Ctrl+S!
