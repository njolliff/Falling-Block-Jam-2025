using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private BoxCollider2D _physicsCollider;
    public float fallSpeed = 1;

    void Update()
    {
        // Destroy the block if below the screen
        if (_physicsCollider != null && IsBelowScreen(_physicsCollider.bounds.max)) Destroy(gameObject);
    }
    void FixedUpdate()
    {
        // Fall at the block's fall speed
        if (_rb != null) _rb.MovePosition(_rb.position + Vector2.down * (fallSpeed / 100));
    }

    private bool IsBelowScreen(Vector3 pos)
    {
        // Get the position relative to the camera's screen space
        Vector3 cameraViewportPos = Camera.main.WorldToViewportPoint(pos);

        // Return true if the object is in below the vertical space of the screen
        if (cameraViewportPos.y < 0) return true;

        // Return false otherwise
        return false;
    }
}