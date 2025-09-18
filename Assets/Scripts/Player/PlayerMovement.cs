using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    // Serialized
    [Header("References")]
    [SerializeField] private StartingPlatform _startingPlatform;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;

    [Header("Movement Variables")]
    public float moveSpeed = 10;
    public Vector2 maxVelocity = new(5, 8);
    public float jumpStrength = 5;
    public float wallJumpStrength = 1;
    public float dashStrength = 10;
    public float dashCooldown = 0.75f;
    public float dashDuration = 0.5f;

    [Header("Debug")]
    public Vector2 movementInput;
    public Vector2 strongestInput;
    public bool grounded = true, wallSliding = false, facingRight = true;

    // Non-Serialized
    public static Vector3 playerPos { get; private set; }
    private bool _canMove = true, _canJump = true, _canDash = true, _dashOnCooldown = false, _isDashing = false, _inKnockback = false, _limitVelocityX = true, _limitVelocityY = true;
    private float _dashCooldownTimer = 0f, _dashDurationTimer = 0f;
    #endregion

    #region Update
    void Update()
    {
        UpdateCooldowns();

        DoRaycasts();

        UpdatePlayerOrientation();
    }
    void FixedUpdate()
    {
        MovePlayer();
        
        LimitVelocity();
    }
    #endregion

    #region Main Methods
    private void MovePlayer()
    {
        if (_rb != null && _canMove)
        {
            // Stop if there is no horizontal input
            if (movementInput == Vector2.zero) _rb.linearVelocityX = 0;

            // If below the max velocity on the X axis, add force equal to moveSpeed on the X axis
            if (Mathf.Abs(_rb.linearVelocityX) < maxVelocity.x) _rb.AddForceX(moveSpeed * movementInput.x, ForceMode2D.Force);
        }
    }
    private void LimitVelocity()
    {
        // Cap velocity on the X axis if limitVelocityX is true
        if (_limitVelocityX && Mathf.Abs(_rb.linearVelocityX) > maxVelocity.x)
            _rb.linearVelocityX = (_rb.linearVelocityX >= maxVelocity.x) ? maxVelocity.x : -maxVelocity.x;

        // Cap velocity on the Y axis if limitVelocityY is true
        if (_limitVelocityY && Mathf.Abs(_rb.linearVelocityY) > maxVelocity.y)
            _rb.linearVelocityY = (_rb.linearVelocityY >= maxVelocity.y) ? maxVelocity.y : -maxVelocity.y;
    }
    private void Dash()
    {
        if (_rb == null) return; // Rb null check

        // Update state and cooldown bools
        if (!grounded && !wallSliding) _canDash = false; // Only disable dash if not dashing from the ground or a wall
        _canMove = false;
        _dashOnCooldown = true;
        _isDashing = true;
        _limitVelocityX = false;

        // Stop movement then apply force so that dash strength is the same every time and stops falls
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(new Vector2(movementInput.x, 0) * dashStrength, ForceMode2D.Impulse);
    }
    private void Jump()
    {
        if (_rb == null) return; // Rb null check

        // Switch starting platform to death box on first jump
        if (!_startingPlatform.isDeathBox) _startingPlatform.SwitchToDeathBox();

        // Disable jump if not jumping from the ground or a wall
        if (!grounded && !wallSliding) _canJump = false;

        // If wall sliding, first apply a small horizontal force in the opposite direction of the wall
        if (wallSliding)
        {
            if (facingRight) _rb.AddForce(Vector2.left * wallJumpStrength, ForceMode2D.Impulse); // If facing right, apply force left
            else _rb.AddForce(Vector2.right * wallJumpStrength, ForceMode2D.Impulse); // If facing left, apply force right
        }

        // Stop vertical movement then apply force so that jump strength is the same every time
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
    }
    #endregion
    #region Helper Methods
    // Raycasts
    private void DoRaycasts()
    {
        DetermineGrounded();
        DetermineWallSliding();
    }
    private void DetermineGrounded()
    {
        if (_collider == null) return; // Collider null check

        // Calculate 3 ray origins at the player's feet
        Vector2 middleRayOrigin = new(_collider.bounds.center.x, _collider.bounds.min.y);
        Vector2 leftRayOrigin = middleRayOrigin + Vector2.left * (_collider.bounds.size.x / 2);
        Vector2 rightRayOrigin = middleRayOrigin + Vector2.right * (_collider.bounds.size.x / 2);

        // Set the ray direction, length, and layer mask
        Vector2 rayDirection = Vector2.down;
        LayerMask groundLayer = LayerMask.GetMask("Block");
        float rayLength = 0.05f;

        // Do the raycasts
        RaycastHit2D middleHit = Physics2D.Raycast(middleRayOrigin, rayDirection, rayLength, groundLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(leftRayOrigin, rayDirection, rayLength, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRayOrigin, rayDirection, rayLength, groundLayer);

        // If any ray hits ground, set grounded to true and enable jump and dash
        if ((middleHit.collider != null && middleHit.collider.CompareTag("Ground")) || (leftHit.collider != null && leftHit.collider.CompareTag("Ground")) || (rightHit.collider != null && rightHit.collider.CompareTag("Ground")))
        {
            if (!grounded) grounded = true;
            if (!_canJump) _canJump = true;
            if (!_canDash) _canDash = true;

            // Draw debug rays
            Debug.DrawRay(middleRayOrigin, rayDirection * rayLength, (middleHit.collider != null) ? Color.green : Color.red);
            Debug.DrawRay(leftRayOrigin, rayDirection * rayLength, (leftHit.collider != null) ? Color.green : Color.red);
            Debug.DrawRay(rightRayOrigin, rayDirection * rayLength, (rightHit.collider != null) ? Color.green : Color.red);
        }
        // If no ray hits the ground, set grounded to false and draw red debug rays
        else
        {
            if (grounded) grounded = false;
            Debug.DrawRay(middleRayOrigin, rayDirection * rayLength, Color.red);
            Debug.DrawRay(leftRayOrigin, rayDirection * rayLength, Color.red);
            Debug.DrawRay(rightRayOrigin, rayDirection * rayLength, Color.red);
        }
    }
    private void DetermineWallSliding()
    {
        if (_collider == null) return; // Collider null check

        // Calculate the origins of upper and lower raycasts
        Vector2 upperRayOrigin = new(_collider.bounds.center.x, _collider.bounds.center.y + _collider.bounds.size.y / 2f);
        Vector2 middleRayOrigin = _collider.bounds.center;
        Vector2 lowerRayOrigin = new(_collider.bounds.center.x, _collider.bounds.center.y - _collider.bounds.size.y / 2f);

        // Determine the rays' direction and set their length
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        float rayLength = 0.3f;
        LayerMask wallLayer = LayerMask.GetMask("Block");

        // Do the raycasts
        RaycastHit2D upperHit = Physics2D.Raycast(upperRayOrigin, rayDirection, rayLength, wallLayer);
        RaycastHit2D middleHit = Physics2D.Raycast(middleRayOrigin, rayDirection, rayLength, wallLayer);
        RaycastHit2D lowerHit = Physics2D.Raycast(lowerRayOrigin, rayDirection, rayLength, wallLayer);

        // If any ray hits a wall, set wallSliding to true and enable jump and dash
        if ((upperHit.collider != null && upperHit.collider.CompareTag("Wall")) || (middleHit.collider != null && middleHit.collider.CompareTag("Wall")) || (lowerHit.collider != null && lowerHit.collider.CompareTag("Wall")))
        {
            // Set bools
            if (!wallSliding) wallSliding = true;
            if (!_canJump) _canJump = true;
            if (!_canDash) _canDash = true;

            // Draw debug rays
            Debug.DrawRay(upperRayOrigin, rayDirection * rayLength, (upperHit.collider != null) ? Color.green : Color.red);
            Debug.DrawRay(middleRayOrigin, rayDirection * rayLength, (middleHit.collider != null) ? Color.green : Color.red);
            Debug.DrawRay(lowerRayOrigin, rayDirection * rayLength, (lowerHit.collider != null) ? Color.green : Color.red);
        }
        // If no ray hits a wall, set wallSliding to false and draw red debug rays
        else
        {
            if (wallSliding) wallSliding = false;
            Debug.DrawRay(upperRayOrigin, rayDirection * rayLength, Color.red);
            Debug.DrawRay(middleRayOrigin, rayDirection * rayLength, Color.red);
            Debug.DrawRay(lowerRayOrigin, rayDirection * rayLength, Color.red);
        }
    }
    // Player State Updaters
    private void UpdatePlayerOrientation()
    {
        // Update player position
        playerPos = transform.position;

        // Flip the character and updating facingRight to match the movement input on the X axis
        if (movementInput.x > 0)
        {
            if (transform.rotation.y != 0) transform.rotation = new(0, 0, 0, 0);
            if (!facingRight) facingRight = true;
        }
        else if (movementInput.x < 0)
        {
            if (transform.rotation.y != 180) transform.rotation = new(0, 180, 0, 0);
            if (facingRight) facingRight = false;
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
                // Enable movement and velocity limit on X axis if they are not also being disabled because the player is taking knockback
                if (!_inKnockback)
                {
                    _canMove = true;
                    _limitVelocityX = true;
                }
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
    // Input Helpers
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
        if (input == Vector2.zero) return strongestInput;

        // If on a controller, determine if the player is more strongly inputting on the Y axis or X axis, then set that axis to 1/-1
        // If they are input at equal strength, favor the X axis
        if (_playerInput.currentControlScheme == "Gamepad" || _playerInput.currentControlScheme == "XR")
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x)) strongestInput.y = (input.y > 0) ? 1 : -1;
            else strongestInput.x = (input.x > 0) ? 1 : -1;
        }
        // If on KBM, input strength is always -1, 0, or 1, so favor the Y axis to allow the player to use Up/Down attacks while moving left/right
        else if (_playerInput.currentControlScheme == "KeyboardMouse")
        {
            if (input.y != 0) strongestInput.y = (input.y > 0) ? 1 : -1;
            else strongestInput.x = (input.x > 0) ? 1 : -1;
        }

        return strongestInput;
    }
    #endregion

    #region Input Functions
    public void OnMove(InputValue inputValue)
    {
        // Get the raw Vector2 input
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