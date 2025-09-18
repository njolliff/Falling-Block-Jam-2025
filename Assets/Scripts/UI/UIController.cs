using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _GameplayUI;
    [SerializeField] private GameObject _deathUI;

    void OnEnable()
    {
        EventManager.onPlayerDied += EnableDeathUI;
        EventManager.onPlayerRespawned += EnableGameplayUI;
    }
    void OnDisable()
    {
        EventManager.onPlayerDied -= EnableDeathUI;
        EventManager.onPlayerRespawned -= EnableGameplayUI;
    }

    private void EnableDeathUI()
    {
        _GameplayUI.SetActive(false); // Disable gameplay UI
        _deathUI.SetActive(true); // Enable death UI
    }
    private void EnableGameplayUI()
    {
        _deathUI.SetActive(false); // Disable death UI
        _GameplayUI.SetActive(true); // Enable gameplay UI
    }
}