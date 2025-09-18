using System;

public static class EventManager
{
    #region Player Events
    // Player died
    public static event Action onPlayerDied;
    public static void PlayerDied() => onPlayerDied?.Invoke();
    // Player respawned
    public static event Action onPlayerRespawned;
    public static void PlayerRespawned() => onPlayerRespawned?.Invoke();
    #endregion
}