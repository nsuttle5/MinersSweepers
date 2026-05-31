using UnityEngine;

public class MapSceneInit : MonoBehaviour
{
    void Start()
    {
        var mapRootObj = GameObject.Find("MapRoot");
        if (mapRootObj != null && MapManager.Instance != null)
        {
            MapManager.Instance.RebuildMapUI(mapRootObj.transform);
        }
    }
}