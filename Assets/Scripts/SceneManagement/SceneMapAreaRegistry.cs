using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SceneMapAreaRegistry : MonoBehaviour
{
    static SceneMapAreaRegistry instance;

    readonly Dictionary<string, MapArea> mapAreasByScene = new();

    void Awake()
    {
        instance = this;
        RegisterMapAreas();
    }

    private void RegisterMapAreas()
    {
        foreach (var trigger in FindObjectsByType<SceneDetails>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!string.IsNullOrEmpty(trigger.SceneName) && trigger.MapArea != null)
            {
                mapAreasByScene.Add(trigger.SceneName, trigger.MapArea);
            }
        }
    }

    public static MapArea GetMapAreaForScene(Scene scene)
    {
        if (instance == null)
            return null;
        if (instance.mapAreasByScene.Count == 0) // If this awoke before any scenes were loaded.
            instance.RegisterMapAreas();

        instance.mapAreasByScene.TryGetValue(scene.name, out var area);
        return area;
    }
}
