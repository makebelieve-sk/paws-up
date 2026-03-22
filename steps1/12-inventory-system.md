# Step 12 — Inventory System [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6. Step 12 of 21. Depends on: Step 11 (interaction).

## Task
Create an inventory system with ScriptableObject items, singleton manager, and events.

## 1. ItemData ScriptableObject

Create `Assets/_PawsUp/Scripts/Inventory/ItemData.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Inventory
{
    public enum ItemCategory
    {
        Clue,
        Consumable,
        KeyItem,
        Reward
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "PawsUp/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public ItemCategory category;
        public GameObject modelPrefab; // for 3D preview
        public int maxStack = 1;

        [Header("Gameplay")]
        public bool isQuestItem; // cannot be discarded
    }
}
```

## 2. InventorySlot

Create `Assets/_PawsUp/Scripts/Inventory/InventorySlot.cs`:

```csharp
namespace PawsUp.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;

        public InventorySlot(ItemData item, int count = 1)
        {
            this.item = item;
            this.count = count;
        }

        public bool IsFull => count >= item.maxStack;
    }
}
```

## 3. InventoryManager

Create `Assets/_PawsUp/Scripts/Inventory/InventoryManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PawsUp.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private int maxSlots = 24; // 4x6 grid

        private List<InventorySlot> _slots = new List<InventorySlot>();
        public IReadOnlyList<InventorySlot> Slots => _slots;

        [Header("Events")]
        public UnityEvent<ItemData> OnItemAdded;
        public UnityEvent<ItemData> OnItemRemoved;
        public UnityEvent OnInventoryChanged;

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

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null) return false;

            // Try stacking first
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].item == item && !_slots[i].IsFull)
                {
                    int canAdd = item.maxStack - _slots[i].count;
                    int toAdd = Mathf.Min(amount, canAdd);
                    _slots[i].count += toAdd;
                    amount -= toAdd;
                    if (amount <= 0) break;
                }
            }

            // Add new slots for remainder
            while (amount > 0 && _slots.Count < maxSlots)
            {
                int toAdd = Mathf.Min(amount, item.maxStack);
                _slots.Add(new InventorySlot(item, toAdd));
                amount -= toAdd;
            }

            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] Added: {item.displayName}. Total items: {_slots.Count}");
            return amount <= 0;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].item != item) continue;

                int toRemove = Mathf.Min(amount, _slots[i].count);
                _slots[i].count -= toRemove;
                amount -= toRemove;

                if (_slots[i].count <= 0)
                    _slots.RemoveAt(i);

                if (amount <= 0) break;
            }

            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return amount <= 0;
        }

        public bool HasItem(string itemId)
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].item.itemId == itemId) return true;
            return false;
        }

        public int GetItemCount(string itemId)
        {
            int total = 0;
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].item.itemId == itemId) total += _slots[i].count;
            return total;
        }

        public ItemData GetItemData(string itemId)
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].item.itemId == itemId) return _slots[i].item;
            return null;
        }

        public void Clear()
        {
            _slots.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}
```

## 4. Create Item ScriptableObject Assets

In `Assets/_PawsUp/ScriptableObjects/Items/`, create via right-click → Create → PawsUp → Item Data:

### clue_blue_cloth
- itemId: `clue_blue_cloth`
- displayName: `Клочок синей ткани`
- description: `Найден в щели под задней дверью лавки Жана. Синяя грубая ткань — похоже на кусок чьей-то одежды или попоны.`
- category: Clue
- isQuestItem: true
- maxStack: 1
- icon: (placeholder — create a small blue square sprite or leave null for now)

### cheese_chunk (for future use with Rene)
- itemId: `cheese_chunk`
- displayName: `Кусочек сыра`
- description: `Ароматный кусочек камамбера. Крысы его обожают.`
- category: Consumable
- maxStack: 5

### sausage_piece (for future use as distraction)
- itemId: `sausage_piece`
- displayName: `Кусочек колбасы`
- description: `Маленький кусочек копчёной колбасы. Отвлекает бульдогов.`
- category: Consumable
- maxStack: 3

## 5. Add InventoryManager to Managers

In the Bootstrap scene, add `InventoryManager` component to the `--- MANAGERS ---` GameObject (same object as GameManager).

## Verification
- No compile errors
- `InventoryManager.Instance.AddItem(clueItem)` → item appears in Slots list
- `InventoryManager.Instance.HasItem("clue_blue_cloth")` → returns true
- `InventoryManager.Instance.GetItemCount("cheese_chunk")` → correct count after adding multiple
- OnItemAdded event fires
- PickupObject from step 11 → calls AddItem → item in inventory
