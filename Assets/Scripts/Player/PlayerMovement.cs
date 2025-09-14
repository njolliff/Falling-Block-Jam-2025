using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    // Serialized
    [Header("Components")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;

    [Header("Movement Variables")]
    public float moveSpeed;
    public float maxVelocity;
    public float jumpStrength;
    public float dashStrength;
    public float dashCooldown;
    public float dashDuration;

    // Non-Serialized
    public static Vector3 playerPos { get; private set; }
    [NonSerialized] public Vector2 movementInput, strongestInput;
    [NonSerialized] public bool grounded;
    private bool _canJump = true, _canDash = true, _isDashing = false, _limitVelocity = true;
    private float _dashCooldownTimer = 0f, _dashDurationTimer = 0f;
    #endregion

    #region Unity Functions
    void Update()
    {
        UpdateCooldowns();

        DetermineGrounded();

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
        // If below max velocity, add force
        if (Mathf.Abs(_rb.linearVelocity.magnitude) < maxVelocity)
            _rb.AddForce(new Vector2(movementInput.x, 0) * moveSpeed, ForceMode2D.Force);
        // Otherwise, if _limitVelocity is true, clamp velocity
        else if (_limitVelocity)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxVelocity;
    }
    private void Dash()
    {
        // Disable dash and velocity limit
        _canDash = false;
        _isDashing = true;
        _limitVelocity = false;

        // Stop movement then apply force so that dash strength is the same every time
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(new Vector2(movementInput.x, 0) * dashStrength, ForceMode2D.Impulse);
    }
    private void Jump()
    {
        // Disable jump
        _canJump = false;

        // Stop vertical movement then apply force so that jump strength is the same every time
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
    }
    private void DetermineGrounded()
    {
        // Get the raycast's origin as the point at the player's feet
        Vector3 rayOrigin = new(_collider.bounds.center.x, _collider.bounds.min.y, 0);
        float rayLength = 0.1f;

        // Do the raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, LayerMask.GetMask("Ground"));

        // If the ray hits ground, set grounded to true and enable jump
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            if (!grounded) grounded = true;
            if (!_canJump) _canJump = true;

            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green); // Draw a green debug ray
        }
        else Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red); // If the ray doesn't hit the ground, draw a red debug ray
    }
    private void UpdatePlayerOrientation()
    {
        // TODO: Flip player sprite when switching between facing left/right
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
        if (!_canDash)
        {
            _dashCooldownTimer += Time.deltaTime;
            if (_dashCooldownTimer >= dashCooldown)
            {
                _canDash = true;
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
        if (_canDash)
            Dash();
    }

    public void OnJump()
    {
        if (_canJump)
            Jump();
    }
    #endregion
}