# Step 03 — Pierre's Character Controller [Cursor Agent]

## Context
Game: "Paws Up!" — 3D detective adventure, Unity 6 (6000.3.11f1).
Step 3 of 21. Depends on: Step 02 (input system).

Reference: `Assets/SourceFiles/Scripts/ThirdPersonController.cs` — adapt patterns from this file.

Pierre is a cat. He's smaller than a human (CharacterController height ~0.8m, radius ~0.3m).

## Task
Create Pierre's character controller with walking, running, crouching, jumping, and movement locking for dialogues/cutscenes.

## 1. PierreController.cs

Create `Assets/_PawsUp/Scripts/Player/PierreController.cs`:

```csharp
using UnityEngine;
using PawsUp.Core;

namespace PawsUp.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PawsUpInputs))]
    public class PierreController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float crouchSpeed = 1.0f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 10.0f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float groundedOffset = -0.1f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayers;

        [Header("State")]
        public bool IsMovementLocked;

        // Components
        private CharacterController _controller;
        private PawsUpInputs _input;
        private Animator _animator;
        private Camera _mainCamera;

        // State
        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private bool _grounded;
        private bool _isCrouching;
        private bool _hasAnimator;

        // Animator hashes
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimJump = Animator.StringToHash("Jump");
        private static readonly int AnimFreeFall = Animator.StringToHash("FreeFall");
        private static readonly int AnimCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int AnimMotionSpeed = Animator.StringToHash("MotionSpeed");

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PawsUpInputs>();
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (IsMovementLocked)
            {
                // Still apply gravity when locked
                ApplyGravity();
                UpdateAnimator(0f);
                return;
            }

            GroundCheck();
            HandleCrouch();
            ApplyGravity();
            HandleJump();
            Move();
        }

        private void GroundCheck()
        {
            Vector3 spherePos = new Vector3(
                transform.position.x,
                transform.position.y - groundedOffset,
                transform.position.z
            );
            _grounded = Physics.CheckSphere(spherePos, groundCheckRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator) _animator.SetBool(AnimGrounded, _grounded);
        }

        private void HandleCrouch()
        {
            _isCrouching = _input.crouch;
            if (_hasAnimator) _animator.SetBool(AnimCrouching, _isCrouching);

            // Adjust controller height
            _controller.height = _isCrouching ? 0.5f : 0.8f;
            _controller.center = new Vector3(0, _controller.height / 2f, 0);
        }

        private void HandleJump()
        {
            if (_input.jump && _grounded && !_isCrouching)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (_hasAnimator) _animator.SetTrigger(AnimJump);
            }
        }

        private void ApplyGravity()
        {
            if (_grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            if (_hasAnimator) _animator.SetBool(AnimFreeFall, !_grounded && _verticalVelocity < 0);
        }

        private void Move()
        {
            float targetSpeed = _isCrouching ? crouchSpeed :
                                _input.sprint ? runSpeed : walkSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0f;

            float currentHSpeed = new Vector3(
                _controller.velocity.x, 0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            if (currentHSpeed < targetSpeed - speedOffset ||
                currentHSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed,
                Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                    + _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, _targetRotation,
                    ref _rotationVelocity, rotationSmoothTime);

                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime
            );

            UpdateAnimator(_animationBlend);
        }

        private void UpdateAnimator(float blend)
        {
            if (!_hasAnimator) return;
            _animator.SetFloat(AnimSpeed, blend);
            _animator.SetFloat(AnimMotionSpeed, 1f);
        }
    }
}
```

## 2. Pierre Prefab (Placeholder)

Create a prefab `Assets/_PawsUp/Prefabs/Player/Pierre.prefab`:
- Root GO "Pierre" at position (0, 0, 0)
  - CharacterController: height 0.8, radius 0.3, center (0, 0.4, 0)
  - PierreController component
  - PawsUpInputs component
  - PlayerInput component (Actions: PawsUpActions, Default Map: Player)
  - Child "Model" — Capsule (scale 0.3, 0.4, 0.3), remove Capsule Collider
    - Material: create `Pierre_Placeholder` material, color orange (#FF8C00)
  - Child "HeadIndicator" — small Sphere at (0, 0.7, 0.15), scale 0.15
    - Shows which direction Pierre faces

## 3. Test Scene

Open `Assets/_PawsUp/Scenes/PierresAttic.unity` (or create if not exists).
- Place Pierre prefab at (0, 0.5, 0)
- Add a Plane at (0, 0, 0) scale (5, 1, 5) as ground
- Set ground layer to "Default" or create "Ground" layer
- Set PierreController groundLayers to include the ground layer

## Verification
- WASD moves Pierre, movement is relative to camera direction
- Shift makes Pierre run (faster)
- Ctrl toggles crouch (slower, controller shorter)
- Space jumps (only when grounded, not when crouching)
- Pierre rotates smoothly toward movement direction
- Without Animator — no errors, just no animations
- Set IsMovementLocked = true in Inspector → Pierre stops responding to input
