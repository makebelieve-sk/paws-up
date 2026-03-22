# Step 02 — Input System Actions [Cursor Agent]

## Context
Game: "Paws Up!" — 3D detective adventure, Unity 6 (6000.3.11f1).
Step 2 of 21. Depends on: Step 01 (project structure).

The project already has Input System 1.19.0 installed. We need our own Input Actions.

## Task
Create Input Actions asset and a C# wrapper for all player inputs.

## 1. Input Actions Asset

Create `Assets/_PawsUp/Data/PawsUpActions.inputactions` with two Action Maps:

### Action Map: Player
| Action | Type | Bindings | Description |
|--------|------|----------|-------------|
| Move | Value (Vector2) | WASD, Left Stick | Movement |
| Look | Value (Vector2) | Mouse Delta, Right Stick | Camera rotation |
| Sprint | Button | Left Shift, Left Stick Press | Run |
| Jump | Button | Space, South Button | Jump |
| Crouch | Button | Left Ctrl, East Button | Crouch/sneak |
| Interact | Button | E, West Button | Interact with objects |
| SmellSense | Button | Q, Left Trigger | Activate cat smell |
| Inventory | Button | Tab, North Button | Open/close inventory |
| ThrowItem | Button | G, Right Trigger | Throw distraction |
| Pause | Button | Escape, Start Button | Pause menu |

### Action Map: UI
| Action | Type | Bindings |
|--------|------|----------|
| Navigate | Value (Vector2) | WASD, D-Pad, Left Stick |
| Submit | Button | Enter, South Button |
| Cancel | Button | Escape, East Button |
| Click | Button | Left Mouse Button |
| Point | Value (Vector2) | Mouse Position |

Enable "Generate C# Class" in the asset settings:
- File: `Assets/_PawsUp/Scripts/Core/PawsUpActions.cs`
- Class Name: `PawsUpActions`
- Namespace: `PawsUp.Core`

## 2. Input Wrapper

Create `Assets/_PawsUp/Scripts/Core/PawsUpInputs.cs`:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace PawsUp.Core
{
    public class PawsUpInputs : MonoBehaviour
    {
        [Header("Player Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool sprint;
        public bool jump;
        public bool crouch;
        public bool interact;
        public bool smellSense;
        public bool inventory;
        public bool throwItem;
        public bool pause;

        [Header("Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private PawsUpActions _actions;

        private void OnEnable()
        {
            _actions = new PawsUpActions();
            _actions.Player.Enable();

            _actions.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            _actions.Player.Move.canceled += _ => move = Vector2.zero;

            _actions.Player.Look.performed += ctx => look = ctx.ReadValue<Vector2>();
            _actions.Player.Look.canceled += _ => look = Vector2.zero;

            _actions.Player.Sprint.performed += _ => sprint = true;
            _actions.Player.Sprint.canceled += _ => sprint = false;

            _actions.Player.Jump.performed += _ => jump = true;
            _actions.Player.Crouch.performed += _ => crouch = !crouch; // toggle
            _actions.Player.Interact.performed += _ => interact = true;
            _actions.Player.SmellSense.performed += _ => smellSense = true;
            _actions.Player.Inventory.performed += _ => inventory = true;
            _actions.Player.ThrowItem.performed += _ => throwItem = true;
            _actions.Player.Pause.performed += _ => pause = true;
        }

        private void OnDisable()
        {
            _actions?.Dispose();
        }

        private void LateUpdate()
        {
            // Reset one-shot inputs
            jump = false;
            interact = false;
            smellSense = false;
            inventory = false;
            throwItem = false;
            pause = false;
        }

        public void SwitchToUI()
        {
            _actions.Player.Disable();
            _actions.UI.Enable();
            SetCursorState(false);
        }

        public void SwitchToPlayer()
        {
            _actions.UI.Disable();
            _actions.Player.Enable();
            SetCursorState(true);
        }

        private void SetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            cursorLocked = locked;
        }
    }
}
```

## 3. Unity Editor Setup

In Project Settings:
1. Edit → Project Settings → Player → Other Settings → Active Input Handling → **Both** (to support both old and new)
2. Open the `.inputactions` asset → click "Generate C# Class" checkbox → Apply

## Verification
- No compile errors
- Attach `PawsUpInputs` to a test GameObject
- Enter Play Mode → press WASD → check `move` values in Inspector
- Press E → `interact` flashes true for one frame
- Press Q → `smellSense` flashes true
- Press Tab → `inventory` flashes true
