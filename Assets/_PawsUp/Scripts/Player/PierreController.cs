using UnityEngine;
using PawsUp.Core;

namespace PawsUp.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PawsUpInputs))]
    public class PierreController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float runSpeed = 3.0f;
        [SerializeField] private float crouchSpeed = 0.6f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 8.0f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayers;

        [Header("State")]
        public bool IsMovementLocked;

        private CharacterController _controller;
        private PawsUpInputs _input;
        private Animator _animator;
        private Camera _mainCamera;
        private Transform _model;
        private Transform _headIndicator;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private bool _grounded;
        private bool _isCrouching;
        private bool _wasCrouching;
        private bool _hasAnimator;
        private bool _jumped;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimJump = Animator.StringToHash("Jump");
        private static readonly int AnimFreeFall = Animator.StringToHash("FreeFall");
        private static readonly int AnimCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int AnimMotionSpeed = Animator.StringToHash("MotionSpeed");

        private readonly Collider[] _groundHits = new Collider[8];

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PawsUpInputs>();
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _mainCamera = Camera.main;

            _model = transform.Find("Pierre_Full_Body");
            if (_model == null) _model = transform.Find("Model");

            _headIndicator = transform.Find("HeadIndicator");
        }

        private void Start()
        {
            if (CameraSystem.CameraManager.Instance != null)
                CameraSystem.CameraManager.Instance.BindToTarget(transform);
        }

        private void Update()
        {
            if (IsMovementLocked)
            {
                ApplyGravity();
                UpdateAnimator(0f);
                return;
            }

            if (_input == null || _controller == null || _mainCamera == null)
                return;

            GroundCheck();
            HandleCrouch();
            ApplyGravity();
            HandleJump();
            Move();
        }

        private void GroundCheck()
        {
            float ccBottom = transform.position.y + _controller.center.y - _controller.height / 2f;
            Vector3 spherePos = new Vector3(transform.position.x, ccBottom, transform.position.z);

            int count = Physics.OverlapSphereNonAlloc(spherePos, groundCheckRadius,
                _groundHits, groundLayers, QueryTriggerInteraction.Ignore);

            _grounded = false;
            for (int i = 0; i < count; i++)
            {
                if (!_groundHits[i].transform.IsChildOf(transform))
                {
                    _grounded = true;
                    break;
                }
            }

            if (_hasAnimator) _animator.SetBool(AnimGrounded, _grounded);
        }

        private void HandleCrouch()
        {
            _isCrouching = _input.crouch;

            if (_isCrouching != _wasCrouching)
            {
                _wasCrouching = _isCrouching;
                if (_hasAnimator) _animator.SetBool(AnimCrouching, _isCrouching);

                if (_model != null)
                {
                    _model.localScale = _isCrouching
                        ? new Vector3(_model.localScale.x, 0.25f, _model.localScale.z)
                        : new Vector3(_model.localScale.x, 0.4f, _model.localScale.z);

                    _model.localPosition = _isCrouching
                        ? new Vector3(0, 0.25f, 0)
                        : new Vector3(0, 0.4f, 0);
                }

                if (_headIndicator != null)
                {
                    _headIndicator.localPosition = _isCrouching
                        ? new Vector3(0, 0.45f, 0.15f)
                        : new Vector3(0, 0.7f, 0.15f);
                }
            }
        }

        private void HandleJump()
        {
            if (_jumped)
            {
                if (_grounded) _jumped = false;
                return;
            }

            if (_input.jump && _grounded)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumped = true;
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

            if (!_grounded && !_isCrouching && targetSpeed < _speed)
                targetSpeed = _speed;

            _speed = Mathf.MoveTowards(_speed, targetSpeed, Time.deltaTime * speedChangeRate);

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

            _controller.Move(
                transform.forward * (_speed * Time.deltaTime)
                + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime
            );

            UpdateAnimator(_animationBlend);
        }

        private void UpdateAnimator(float blend)
        {
            if (!_hasAnimator) return;

            float normalizedSpeed = 0f;
            if (blend > 0.01f)
            {
                normalizedSpeed = blend <= walkSpeed
                    ? Mathf.Lerp(0f, 0.5f, blend / walkSpeed)
                    : Mathf.Lerp(0.5f, 1f, (blend - walkSpeed) / (runSpeed - walkSpeed));
            }
            _animator.SetFloat(AnimSpeed, normalizedSpeed);
            _animator.SetFloat(AnimMotionSpeed, 1f);
        }
    }
}
