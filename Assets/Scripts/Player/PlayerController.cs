using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    // PUBLIC
    public static PlayerController Instance { get; private set; }
    [NonSerialized] public float maxHealth;
    public float health = 3;
    public float height = 0;
    public bool isAlive = true;

    // PRIVATE
    private Vector3 _startingPos;
    #endregion

    #region Initialization / Destruction
    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set starting position
        _startingPos = transform.position;

        // Set max health
        maxHealth = health;
    }
    void OnDestroy()
    {
        // Singleton
        if (Instance == this) Instance = null;
    }
    #endregion

    #region Main
    void Update()
    {
        // Update height variable and check that player is not below the starting point
        UpdateHeight();
    }
    #endregion

    #region Helper Methods
    private void UpdateHeight()
    {
        // Update height, rounded to one decimal place
        height = Mathf.Round((transform.position.y - _startingPos.y) * 10) / 10;

        // Check if the player has fallen bellow the starting position, with a small amount of leeway
        if (height <= -1) GameManager.Instance.KillPlayer();
    }

    public void ResetPosition()
    {
        transform.position = _startingPos;
    }
    #endregion
}