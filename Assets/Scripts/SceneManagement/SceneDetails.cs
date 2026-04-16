using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] TileBase backgroundTile;

    Tilemap ledgeTilemap;
    Tilemap solidTilemap;
    Tilemap waterTilemap;

    public void BindTilemaps(Tilemap ledges, Tilemap solids, Tilemap water)
    {
        ledgeTilemap = ledges;
        solidTilemap = solids;
        waterTilemap = water;
    }

    public void UnbindTilemaps()
    {
        ledgeTilemap = null;
        solidTilemap = null;
        waterTilemap = null;
    }

    public string SceneName => gameObject.name;
    public bool IsLoaded { get; private set; }
    List<SavableEntity> savableEntities;
    public MapArea MapArea { get; private set; }
    public List<SceneDetails> ConnectedScenes => connectedScenes;

    void Awake()
    {
        MapArea = GetComponentInParent<MapArea>();
        if (MapArea == null)
            Debug.LogWarning($"SceneDetails on {name} has no MapArea.");
    }

    public SceneEntryPoint GetEntryPoint(string id)
    {
        return FindObjectsByType<SceneEntryPoint>()
            .FirstOrDefault(e =>
                e.EntryId == id &&
                e.gameObject.scene.name == SceneName
            );
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
                // FillBackgroundPadding();
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            // Save state for all SavableEntity objects
            var entities = GetSavableEntitiesInScene();
            SavingSystem.i.CaptureEntityStates(entities);

            // Unload scene
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Open Scene")]
    public void OpenSceneInEditor()
    {
        if (!EditorSceneManager.GetSceneByName(gameObject.name).isLoaded)
        {
            string path = $"Assets/Scenes/{gameObject.name}.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }
    }

    [ContextMenu("Close Scene")]
    public void CloseSceneInEditor()
    {
        var scene = EditorSceneManager.GetSceneByName(gameObject.name);
        if (scene.isLoaded)
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    public static List<SceneDetails> FindAllMapScenes()
    {
        var results = new List<SceneDetails>();

        var guids = AssetDatabase.FindAssets("t:Scene");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            // 1️⃣ Exclude package scenes
            if (!path.StartsWith("Assets/Scenes/"))
                continue;

            if (path.Contains("MainMenu"))
                continue;

            // 2️⃣ Open additively (editor-only)
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            // 3️⃣ Find SceneDetails
            foreach (var root in scene.GetRootGameObjects())
            {
                var details = root.GetComponentInChildren<SceneDetails>(true);
                if (details == null)
                    continue;

                // 4️⃣ Exclude persistent / non-map scenes
                // Heuristic: must have a SceneTrigger or bounds
                var trigger = details.GetComponentInParent<SceneTrigger>();
                if (trigger == null)
                    continue;

                results.Add(details);
            }
        }

        return results;
    }

    void OnValidate()
    {
        var scene = gameObject.scene;
        if (!scene.IsValid() || scene.name == "Gameplay")
            return;

        var all = FindObjectsByType<SceneDetails>()
            .Where(s => s.gameObject.scene == scene)
            .ToList();

        if (all.Count > 1)
        {
            Debug.LogError(
                $"Scene '{scene.name}' has multiple SceneDetails components.",
                this
            );
        }
    }
#endif

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsByType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }

    void FillBackgroundPadding()
    {
        if (backgroundTile == null) return;

        var scene = SceneManager.GetSceneByName(gameObject.name);
        var roots = scene.GetRootGameObjects();

        var grid = roots
            .SelectMany(r => r.GetComponentsInChildren<Grid>())
            .FirstOrDefault();

        if (grid == null) return;

        var bgTilemap = grid.transform.Find("Background")
            ?.GetComponent<Tilemap>();

        if (bgTilemap == null) return;

        // Big enough to cover any camera
        int paddingSize = 50;

        for (int x = -paddingSize; x <= paddingSize; x++)
        {
            for (int y = -paddingSize; y <= paddingSize; y++)
            {
                bgTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
            }
        }
    }

    public LedgeTile GetLedgeAtWorldPos(Vector3 worldPos)
    {
        if (ledgeTilemap == null)
            return null;

        var cell = ledgeTilemap.WorldToCell(worldPos);
        return ledgeTilemap.GetTile<LedgeTile>(cell);
    }

    public bool GetWaterAtWorldPos(Vector3 worldPos)
    {
        if (waterTilemap == null)
            return false;

        var cell = waterTilemap.WorldToCell(worldPos);
        return waterTilemap.HasTile(cell);
    }

    public bool IsSolidAtWorldPos(Vector3 worldPos)
    {
        if (solidTilemap == null)
            return false;

        var cell = solidTilemap.WorldToCell(worldPos);
        return solidTilemap.HasTile(cell);
    }
}
