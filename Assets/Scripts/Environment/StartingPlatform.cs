using UnityEngine;

public class StartingPlatform : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public bool isDeathBox = false;

    public void SwitchToDeathBox()
    {
        if (_collider == null || _spriteRenderer == null) return;

        isDeathBox = true; // Change state
        _collider.isTrigger = true; // Change hitbox to trigger
        _spriteRenderer.color = new Color(1, 0, 0, 0.5f); // Set sprite color to semi-transparent red
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDeathBox && collision.CompareTag("Player")) GameManager.Instance.KillPlayer();
    }
}