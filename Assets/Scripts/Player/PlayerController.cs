using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public float health = 3;
    public float height = 0;
    public bool isAlive = true;

    [SerializeField] private Collider2D _physicsCollider;
    private float _startingHeight;

    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set starting height at the player's feet
        float playerHeight = _physicsCollider.bounds.size.y;
        _startingHeight = transform.position.y - (playerHeight / 2);
    }

    void OnDestroy()
    {
        // Singleton
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Update height
        height = transform.position.y - _startingHeight;

        // Check if the player has fallen below the screen
        CheckScreenHeight();
    }

    private void CheckScreenHeight()
    {
        // Get the position of the center top of the player's physics collider
        Vector3 topPos = _physicsCollider.bounds.center + Vector3.up * (_physicsCollider.bounds.size.y / 2);

        // Get the topPos relative to the main camera's viewport
        Vector3 topViewportPos = Camera.main.WorldToViewportPoint(topPos);

        // If below the screen, the player is dead
        if (topViewportPos.y <= 0) KillPlayer();
    }

    private void KillPlayer()
    {
        Time.timeScale = 0; // Pause the game

        if (health != 0) health = 0; // Set health to 0

        // Disable gameplayer UI
        // Enable death UI
    }
}