using UnityEngine;

public class StartingPlatform : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _deathBoxSprite;
    public bool isDeathBox = false;

    public void SwitchToDeathBox()
    {
        if (_collider == null || _spriteRenderer == null) return;

        isDeathBox = true; // Change state
        _collider.isTrigger = true; // Change hitbox to trigger
        _spriteRenderer.sprite = _deathBoxSprite; // Set sprite to death box sprite
        gameObject.layer = LayerMask.GetMask("Death Box"); // Update gameObject layer
        gameObject.tag = "Death Box"; // Update gameObject tag
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDeathBox && collision.CompareTag("Player")) GameManager.Instance.KillPlayer();
    }
}