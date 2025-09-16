using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    // Serialized
    [Header("References")]
    [SerializeField] private GameObject _startingPlatform;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;

    [Header("Movement Variables")]
    public float moveSpeed = 10;
    public float maxVelocity = 8;
    public float jumpStrength = 5;
    public float wallJumpStrength = 1;
    public float dashStrength = 10;
    public float dashCooldown = 0.75f;
    public float dashDuration = 0.5f;

    [Header("Debug")]
    public Vector2 movementInput;
    public Vector2 strongestInput;
    public bool grounded, wallSliding, facingRight = true;

    // Non-Serialized
    public static Vector3 playerPos { get; private set; }
    private bool _canJump = true, _canDash = true, _dashOnCooldown = false, _isDashing = false, _limitVelocity = true;
    private float _dashCooldownTimer = 0f, _dashDurationTimer = 0f;
    #endregion

    #region Unity Functions
    void Update()
    {
        UpdateCooldowns();

        DoRaycasts();

        UpdatePlayerOrientation();

        playerPos = transform.position;
    }
    void FixedUpdate()
    {
        MovePlayer();
    }
    #endregion

    #region Custom Functions
    private void MovePlayer()
    {
        if (_rb == null) return; // Rb null check

        if (movementInput.x == 0 && _limitVelocity) _rb.linearVelocityX = 0; // Halt horizontal movement if there is no movement input on the axis axis and velocity is being limited

        // If below max velocity, add force
        if (Mathf.Abs(_rb.linearVelocity.magnitude) < maxVelocity)
            _rb.AddForce(new Vector2(movementInput.x, 0) * moveSpeed, ForceMode2D.Force);
        // Otherwise, if _limitVelocity is true, clamp velocity
        else if (_limitVelocity)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxVelocity;
    }
    private void Dash()
    {
        if (_rb == null) return; // Rb null check

        // Disable dash and velocity limit
        if (!grounded && !wallSliding) _canDash = false; // Only disable dash if not dashing from the ground or a wall
        _isDashing = true;
        _limitVelocity = false;

        // Stop movement then apply force so that dash strength is the same every time
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(new Vector2(movementInput.x, 0) * dashStrength, ForceMode2D.Impulse);
    }
    private void Jump()
    {
        if (_rb == null) return; // Rb null check

        // Destroy starting platform on first jump
        if (_startingPlatform != null)
        {
            Destroy(_startingPlatform);
            _startingPlatform = null;
        }

        // Disable jump if not jumping from the ground or a wall
        if (!grounded && !wallSliding) _canJump = false;

        // If wall sliding, first apply a small horizontal force in the opposite direction of the wall
        if (wallSliding)
        {
            _limitVelocity = false;
            if (facingRight) _rb.AddForce(Vector2.left * wallJumpStrength, ForceMode2D.Impulse); // If facing right, apply force left
            else _rb.AddForce(Vector2.right * wallJumpStrength, ForceMode2D.Impulse); // If facing left, apply force right
            _limitVelocity = true;
        }

        // Stop vertical movement then apply force so that jump strength is the same every time
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
    }
    private void DoRaycasts()
    {
        DetermineGrounded();
        DetermineWallSliding();
    }
    private void DetermineGrounded()
    {
        if (_collider == null) return; // Collider null check

        // Get the raycast's origin as the point at the player's feet and set the ray's length
        Vector2 rayOrigin = new(_collider.bounds.center.x, _collider.bounds.min.y);
        float rayLength = 0.05f;

        // Do the raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, LayerMask.GetMask("Ground"));

        // If the ray hits ground, set grounded to true and enable jump and dash
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            if (!grounded) grounded = true;
            if (!_canJump) _canJump = true;
            if (!_canDash) _canDash = true;

            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green); // Draw a green debug ray
        }
        // If the ray doesn't hit ground, set grounded to false and draw a red debug ray
        else
        {
            if (grounded) grounded = false;
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
        }
    }
    private void DetermineWallSliding()
    {
        if (_collider == null) return; // Collider null check

        // Calculate the origins of upper and lower raycasts
        Vector2 upperRayOrigin = new(_collider.bounds.center.x, _collider.bounds.center.y + _collider.bounds.size.y / 2.5f);
        Vector2 lowerRayOrigin = new(_collider.bounds.center.x, _collider.bounds.center.y - _collider.bounds.size.y / 2.5f);

        // Determine the rays' direction and set their length
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        float rayLength = 0.3f;

        // Do the raycasts
        RaycastHit2D upperHit = Physics2D.Raycast(upperRayOrigin, rayDirection, rayLength, LayerMask.GetMask("Wall"));
        RaycastHit2D lowerHit = Physics2D.Raycast(lowerRayOrigin, rayDirection, rayLength, LayerMask.GetMask("Wall"));

        // If either ray hits a wall, set wallSliding to true and enable jump and dash
        if ((upperHit.collider != null && upperHit.collider.CompareTag("Wall")) || (lowerHit.collider != null && lowerHit.collider.CompareTag("Wall")))
        {
            if (!wallSliding) wallSliding = true;
            if (!_canJump) _canJump = true;
            if (!_canDash) _canDash = true;

            // Draw debug rays
            if (upperHit.collider != null) Debug.DrawRay(upperRayOrigin, rayDirection * rayLength, Color.green);
            else Debug.DrawRay(upperRayOrigin, rayDirection * rayLength, Color.red);

            if (lowerHit.collider != null) Debug.DrawRay(lowerRayOrigin, rayDirection * rayLength, Color.green);
            else Debug.DrawRay(lowerRayOrigin, rayDirection * rayLength, Color.red);
        }
        // If neither ray hits a wall, set wallSliding to false and draw two red debug rays
        else
        {
            if (wallSliding) wallSliding = false;
            Debug.DrawRay(upperRayOrigin, rayDirection * rayLength, Color.red);
            Debug.DrawRay(lowerRayOrigin, rayDirection * rayLength, Color.red);
        }
    }
    private void UpdatePlayerOrientation()
    {
        if (movementInput.x > 0 && transform.rotation.y != 0)
        {
            transform.rotation = new(0, 0, 0, 0);
            facingRight = true;
        }
        else if (movementInput.x < 0 && transform.rotation.y != 180)
        {
            transform.rotation = new(0, 180, 0, 0);
            facingRight = false;
        }
    }
    private void UpdateCooldowns()
    {
        // Dash duration
        if (_isDashing)
        {
            _dashDurationTimer += Time.deltaTime;
            if (_dashDurationTimer >= dashDuration)
            {
                _isDashing = false;
                _limitVelocity = true;
                _dashDurationTimer = 0f;
            }
        }

        // Dash cooldown
        if (_dashOnCooldown)
        {
            _dashCooldownTimer += Time.deltaTime;
            if (_dashCooldownTimer >= dashCooldown)
            {
                _dashOnCooldown = false;
                _dashCooldownTimer = 0f;
            }
        }
    }
    private Vector2 FlattenInput(Vector2 input)
    {
        // Default return value to (0,0)
        Vector2 flattenedInput = new(0, 0);

        // Update X/Y to 1/-1 if input is +/-
        // X
        if (input.x > 0)
            flattenedInput.x = 1;
        else if (input.x < 0)
            flattenedInput.x = -1;
        // Y
        if (input.y > 0)
            flattenedInput.y = 1;
        else if (input.y < 0)
            flattenedInput.y = -1;

        return flattenedInput;
    }
    private Vector2 DetermineStrongestInput(Vector2 input)
    {
        // Default return value to (0,0)
        Vector2 strongestInput = new(0, 0);

        // Ensure the player is inputting a direction
        if (input == Vector2.zero)
            return strongestInput;

        // If on a controller, determine if the player is more strongly inputting on the Y axis or X axis, then set that axis to 1/-1
        // If they are input at equal strength, favor the X axis
        if (_playerInput.currentControlScheme == "Gamepad" || _playerInput.currentControlScheme == "XR")
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                strongestInput.y = (input.y > 0) ? 1 : -1;
            else
                strongestInput.x = (input.x > 0) ? 1 : -1;
        }
        // If on KBM, input strength is always -1, 0, or 1, so favor the Y axis to allow the player to use Up/Down attacks while moving left/right
        else if (_playerInput.currentControlScheme == "KeyboardMouse")
        {
            if (input.y != 0)
                strongestInput.y = (input.y > 0) ? 1 : -1;
            else
                strongestInput.x = (input.x > 0) ? 1 : -1;
        }

        return strongestInput;
    }
    #endregion

    #region Input Functions
    public void OnMove(InputValue inputValue)
    {
        // Get the raw float input
        Vector2 rawInput = inputValue.Get<Vector2>();

        // Flatten the input to -1, 0, or 1 and determine the strongest input for determining attack direction
        movementInput = FlattenInput(rawInput);
        strongestInput = DetermineStrongestInput(rawInput);
    }

    public void OnDash()
    {
        if (_canDash && !_dashOnCooldown)
            Dash();
    }

    public void OnJump()
    {
        if (_canJump)
            Jump();
    }
    #endregion
}