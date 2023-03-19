using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    [SerializeField] private float _moveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    [SerializeField] private float _sprintSpeed = 5.335f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float _speedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float _gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField] private float _jumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float _fallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    [SerializeField] private bool _grounded = true;
    [Tooltip("Useful for rough ground")]
    [SerializeField] private float _groundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float _groundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask _groundLayers;

    [Header("Camera")]
    [Tooltip("Get a reference to our main camera")]
    [SerializeField] private Camera _camera;

    [Header("References")]
    [SerializeField] private Inputs _inputs;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Player _player;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private EnduranceSystem _enduranceSystem;

    // Player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private bool _rotateOnMove = true;

    // Timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    public void EnableRotateOnMove(bool state)
    {
        _rotateOnMove = state;
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    void Start()
    {
        // Reset our timeouts on start
        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;
    }

    void Update()
    {
        if (_player.GetIsDead())
        {
            return;
        }

        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void GroundedCheck()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        // Update animator
        _playerAnimator.EnableGrounded(_grounded);
    }

    private void Move()
    {
        // Set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _inputs.sprint ? _sprintSpeed : _moveSpeed;
        // Check if player have endurance for speed
        if (_enduranceSystem.GetEndurance() <= 0f)
        {
            targetSpeed = _moveSpeed;
        }

        // A simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // Note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // If there is no input, set the target speed to 0
        if (_inputs.move == Vector2.zero) targetSpeed = 0.0f;

        // A reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _inputs.analogMovement ? _inputs.move.magnitude : 1f;

        // Accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Creates curved result rather than a linear one giving a more organic speed change
            // Note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * _speedChangeRate);

            // Round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);

        // Normalise input direction
        Vector3 inputDirection = new Vector3(_inputs.move.x, 0.0f, _inputs.move.y).normalized;

        // Note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // If there is a move input rotate player when the player is moving
        if (_inputs.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _rotationSmoothTime);

            // Rotate to face input direction relative to camera position
            if (_rotateOnMove)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Update animator
        _playerAnimator.SetSpeedValue(_animationBlend);
        _playerAnimator.SetMotionSpeedValue(inputMagnitude);
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            // Reset the fall timeout timer
            _fallTimeoutDelta = _fallTimeout;

            // Update animator
            _playerAnimator.EnableJump(false);
            _playerAnimator.EnableFreeFall(false);

            // Stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_inputs.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // The square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                // Update animator
                _playerAnimator.EnableJump(true);
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset the jump timeout timer
            _jumpTimeoutDelta = _jumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // Update animator
                _playerAnimator.EnableFreeFall(true);
            }

            // If we are not grounded, do not jump
            _inputs.jump = false;
        }

        // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z), _groundedRadius);
    }
}