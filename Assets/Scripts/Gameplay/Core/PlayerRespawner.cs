using System.Collections;
using UnityEngine;

/// <summary>
/// Respawns the player on a loss after a variable amount of time
/// </summary>
public class PlayerRespawner : MonoBehaviour
{
    [SerializeField] private float _respawnDelay = 3.0f;

    void Start()
    {
        GameStateManager.OnStateWinScreen += RespawnAfterDelay;
        GameStateManager.OnStateLoseScreen += RespawnAfterDelay;
    }

    void OnDestroy()
    {
        GameStateManager.OnStateWinScreen -= RespawnAfterDelay;
        GameStateManager.OnStateLoseScreen -= RespawnAfterDelay;
    }

    private void RespawnAfterDelay()
    {
        StartCoroutine(RespawnAfterDelayRoutine());
    }

    private IEnumerator RespawnAfterDelayRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);

        // we are still on the lose screen
        if(GameStateManager.CurrentState == GameStateManager.GameState.LoseBlackScreenWipe)
        {
            GameStateManager.Singleton.SetState(GameStateManager.GameState.Playing);
        }

        else if(GameStateManager.CurrentState == GameStateManager.GameState.WinWhiteScreenWipe)
        {
            LevelFullMap.Singleton.LoadNextLevel();
            GameStateManager.Singleton.SetState(GameStateManager.GameState.Playing);
        }
    }
}
