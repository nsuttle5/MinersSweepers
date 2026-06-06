using UnityEngine;

public class GameResetter : MonoBehaviour
{
    public void ResetGame()
    {
        if (GameData.HasInstance)
            Destroy(GameData.Instance.gameObject);

        if (PlayerRunStats.HasInstance)
            Destroy(PlayerRunStats.Instance.gameObject);

        if (MapManager.Instance != null)
            Destroy(MapManager.Instance.gameObject);
    }
}
