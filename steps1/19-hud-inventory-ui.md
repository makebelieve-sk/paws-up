# Step 19 — HUD & Inventory UI [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 19 of 21. Depends on: Steps 12, 14, 15.

## Task
Create the HUD (smell bar, quest tracker), inventory panel (Tab), and pause menu (Esc).

## 1. HUDManager

Create `Assets/_PawsUp/Scripts/UI/HUDManager.cs`:

```csharp
using UnityEngine;
using PawsUp.Core;

namespace PawsUp.UI
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [SerializeField] private GameObject hudPanel;
        [SerializeField] private SmellEnergyBar smellBar;
        [SerializeField] private QuestTrackerUI questTracker;

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

        private void Update()
        {
            bool showHUD = GameManager.Instance == null || GameManager.Instance.IsPlaying;
            hudPanel.SetActive(showHUD);
        }
    }
}
```

## 2. SmellEnergyBar

Create `Assets/_PawsUp/Scripts/UI/SmellEnergyBar.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using PawsUp.SmellSense;

namespace PawsUp.UI
{
    public class SmellEnergyBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Color normalColor = new Color(0.4f, 0.8f, 1f);
        [SerializeField] private Color activeColor = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color lowColor = new Color(1f, 0.3f, 0.2f);

        private void Update()
        {
            var smell = SmellSenseManager.Instance;
            if (smell == null || fillImage == null) return;

            fillImage.fillAmount = smell.EnergyNormalized;

            if (smell.IsActive)
                fillImage.color = activeColor;
            else if (smell.EnergyNormalized < 0.2f)
                fillImage.color = lowColor;
            else
                fillImage.color = normalColor;
        }
    }
}
```

## 3. QuestTrackerUI

Create `Assets/_PawsUp/Scripts/UI/QuestTrackerUI.cs`:

```csharp
using UnityEngine;
using TMPro;
using PawsUp.Quest;

namespace PawsUp.UI
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private string trackQuestId = "main_q1";

        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStarted.AddListener(OnQuestChanged);
                QuestManager.Instance.OnObjectiveCompleted.AddListener((q, o) => OnQuestChanged(q));
            }
            UpdateDisplay();
        }

        private void OnQuestChanged(string questId)
        {
            if (questId == trackQuestId) UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var qm = QuestManager.Instance;
            if (qm == null || !qm.IsQuestActive(trackQuestId))
            {
                questTitleText.text = "";
                objectiveText.text = "";
                return;
            }

            questTitleText.text = "Расследовать кражу";
            var current = qm.GetCurrentObjective(trackQuestId);
            objectiveText.text = current != null ? $"► {current.description}" : "Все цели выполнены";
        }
    }
}
```

## 4. InventoryUI

Create `Assets/_PawsUp/Scripts/UI/InventoryUI.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PawsUp.Core;
using PawsUp.Inventory;

namespace PawsUp.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Details")]
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDescription;

        private PawsUpInputs _input;
        private bool _isOpen;

        private void Start()
        {
            _input = FindAnyObjectByType<PawsUpInputs>();
            inventoryPanel.SetActive(false);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged.AddListener(RefreshSlots);
        }

        private void Update()
        {
            if (_input != null && _input.inventory)
                ToggleInventory();
        }

        public void ToggleInventory()
        {
            _isOpen = !_isOpen;
            inventoryPanel.SetActive(_isOpen);

            if (_isOpen)
            {
                GameManager.Instance?.SetState(GameState.Inventory);
                _input?.SwitchToUI();
                RefreshSlots();
                ClearDetails();
            }
            else
            {
                GameManager.Instance?.SetState(GameState.Playing);
                _input?.SwitchToPlayer();
            }
        }

        private void RefreshSlots()
        {
            // Clear existing
            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);

            if (InventoryManager.Instance == null) return;

            foreach (var slot in InventoryManager.Instance.Slots)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var icon = go.GetComponentInChildren<Image>();
                if (icon != null && slot.item.icon != null)
                    icon.sprite = slot.item.icon;

                var countText = go.GetComponentInChildren<TextMeshProUGUI>();
                if (countText != null && slot.count > 1)
                    countText.text = slot.count.ToString();
                else if (countText != null)
                    countText.text = "";

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var item = slot.item;
                    btn.onClick.AddListener(() => ShowDetails(item));
                }
            }
        }

        private void ShowDetails(ItemData item)
        {
            if (detailIcon != null && item.icon != null)
            {
                detailIcon.sprite = item.icon;
                detailIcon.gameObject.SetActive(true);
            }
            detailName.text = item.displayName;
            detailDescription.text = item.description;
        }

        private void ClearDetails()
        {
            if (detailIcon != null) detailIcon.gameObject.SetActive(false);
            detailName.text = "";
            detailDescription.text = "";
        }
    }
}
```

## 5. PauseMenuUI

Create `Assets/_PawsUp/Scripts/UI/PauseMenuUI.cs`:

```csharp
using UnityEngine;
using PawsUp.Core;

namespace PawsUp.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        private PawsUpInputs _input;
        private bool _isPaused;

        private void Start()
        {
            _input = FindAnyObjectByType<PawsUpInputs>();
            pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (_input != null && _input.pause)
                TogglePause();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            pausePanel.SetActive(_isPaused);
            Time.timeScale = _isPaused ? 0f : 1f;

            if (_isPaused)
            {
                GameManager.Instance?.SetState(GameState.Paused);
                _input?.SwitchToUI();
            }
            else
            {
                GameManager.Instance?.SetState(GameState.Playing);
                _input?.SwitchToPlayer();
            }
        }

        public void OnResumeClick() => TogglePause();

        public void OnQuitClick()
        {
            Time.timeScale = 1f;
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
```

## 6. Inventory Slot Prefab

Create prefab `Assets/_PawsUp/Prefabs/UI/InventorySlot.prefab`:
- Root: Button (80x80px), Image component (semi-transparent background)
  - Child: Image "Icon" (60x60px, centered) — for item sprite
  - Child: TextMeshProUGUI "Count" (bottom-right corner, small font)

## 7. Unity Canvas Setup (for step 21)

Create a DontDestroyOnLoad Canvas hierarchy:

```
HUD_Canvas (Screen Space - Overlay, Sort Order: 10)
  ├─ HUD_Panel (anchored full screen)
  │   ├─ SmellEnergyBar (top-left)
  │   │   └─ Fill (Image, Fill Method: Horizontal)
  │   └─ QuestTracker (top-right)
  │       ├─ QuestTitle (TextMeshPro, bold)
  │       └─ Objective (TextMeshPro, smaller)
  ├─ InventoryPanel (center, initially inactive)
  │   ├─ Background (dark semi-transparent)
  │   ├─ SlotsGrid (Grid Layout Group 4 columns, 80px cells)
  │   └─ DetailsPanel (right side)
  │       ├─ DetailIcon
  │       ├─ DetailName
  │       └─ DetailDescription
  ├─ PausePanel (center, initially inactive)
  │   ├─ Background
  │   ├─ Title "ПАУЗА"
  │   ├─ Button "Продолжить"
  │   └─ Button "Выход"
  └─ InteractionPrompt (bottom-center, from step 11)
```

## Verification
- Smell bar fills/depletes with Q activation
- Quest tracker shows current objective, updates when objectives complete
- Tab → inventory opens, shows collected items, click shows details
- Tab again → closes, returns to game
- Esc → pause, game freezes, cursor visible
- Resume button → unpause
- No input conflicts between HUD, inventory, pause
