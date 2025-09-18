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
    public Vector3 startingPos;
    #endregion

    #region Initialization / Destruction
    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set starting position
        startingPos = transform.position;

        // Set max health
        maxHealth = health;
    }
    void OnDestroy()
    {
        // Singleton
        if (Instance == this) Instance = null;
    }
    #endregion

    #region Update
    void Update()
    {
        UpdateHeight();
    }
    #endregion

    #region Helper Methods
    private void UpdateHeight()
    {
        // Update height, rounded to one decimal place
        height = Mathf.Round((transform.position.y - startingPos.y) * 10) / 10;
    }

    public void ResetPosition()
    {
        transform.position = startingPos;
    }
    #endregion
}