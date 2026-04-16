using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Linq;

public class GridRuntimeBindings : MonoBehaviour
{
    void Awake()
    {
        var scene = gameObject.scene;
        var sceneDetails = FindSceneDetailsForScene(scene);
        if (sceneDetails == null)
        {
            Debug.LogWarning($"No SceneDetails found for scene {scene.name}");
            return;
        }

        // Find optional tilemaps
        var ledgeTilemap = transform.Find("Ledges")?.GetComponent<Tilemap>();
        var solidTilemap = transform.Find("SolidObjects")?.GetComponent<Tilemap>();
        var waterTilemap = transform.Find("Water")?.GetComponent<Tilemap>();

        sceneDetails.BindTilemaps(
            ledgeTilemap,
            solidTilemap,
            waterTilemap
        );
    }

    void OnDestroy()
    {
        var sceneDetails = FindSceneDetailsForScene(gameObject.scene);

        sceneDetails?.UnbindTilemaps();
    }

    SceneDetails FindSceneDetailsForScene(Scene scene)
    {
        return FindObjectsByType<SceneDetails>()
            .FirstOrDefault(d => d.SceneName == scene.name);
    }
}
