using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Unpause the game
        Time.timeScale = 1;
    }
    void OnDestroy()
    {
        // Singleton
        if (Instance == this) Instance = null;
    }

    [ContextMenu("Kill Player")]
    public void KillPlayer()
    {
        Time.timeScale = 0; // Pause the game
        PlayerController.Instance.health = 0; // Set player health to 0
        EventManager.PlayerDied();// Call player died event
    }
    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the scene
    }
}