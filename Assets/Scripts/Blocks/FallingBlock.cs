using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _physicsCollider;
    public float fallSpeed = 1;

    void Update()
    {
        CheckDespawn();
    }
    void FixedUpdate()
    {
        Fall();
    }

    #region Main Methods
    private void CheckDespawn()
    {
        if (_physicsCollider == null) return; // Null check physics collider

        // Destroy the block if its highest point is below the screen
        // If the block is using a polygon collider, get the point with the highest Y value
        if (_physicsCollider is PolygonCollider2D polygonCollider)
        {
            if (polygonCollider == null) return;
            if (IsBelowStartingPoint(GetHighestPoint(polygonCollider))) Destroy(gameObject);
        }
        // If not using a polygon collider, use Collider2D.bounds.max
        else if (IsBelowStartingPoint(_physicsCollider.bounds.max)) Destroy(gameObject);
    }
    private void Fall()
    {
        // Fall at the block's fall speed
        if (_rb != null) _rb.MovePosition(_rb.position + Vector2.down * (fallSpeed / 100));
    }
    #endregion
    #region Helper Methods
    private Vector3 GetHighestPoint(PolygonCollider2D collider)
    {
        // Use transform.TransformPoint() to get each point in world space
        Vector3 maxPoint = transform.TransformPoint(collider.points[0]);
        foreach (Vector3 point in collider.points)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            if (worldPoint.y > maxPoint.y) maxPoint = worldPoint;
        }

        return maxPoint;
    }
    private bool IsBelowStartingPoint(Vector3 pos)
    {
        // Return true if the position is below the player's starting point - 1
        if (pos.y < PlayerController.Instance.startingPos.y - 1) return true;

        // Return false otherwise
        return false;
    }
    #endregion
}