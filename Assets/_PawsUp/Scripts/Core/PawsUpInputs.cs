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

            // One-shot button callbacks (toggle/trigger only)

            _actions.Player.Jump.performed += _ => jump = true;
            _actions.Player.Interact.performed += _ => interact = true;
            _actions.Player.SmellSense.performed += _ => smellSense = true;
            _actions.Player.Inventory.performed += _ => inventory = true;
            _actions.Player.ThrowItem.performed += _ => throwItem = true;
            _actions.Player.Pause.performed += _ => pause = true;
        }

        private void OnDisable()
        {
            _actions?.Player.Disable();
            _actions?.UI.Disable();
            _actions?.Dispose();
        }

        private void Update()
        {
            // Read continuous values every frame
            move = _actions.Player.Move.ReadValue<Vector2>();
            look = _actions.Player.Look.ReadValue<Vector2>();
            sprint = _actions.Player.Sprint.IsPressed();
            crouch = _actions.Player.Crouch.IsPressed();
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
