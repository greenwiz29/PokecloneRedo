using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] TileBase backgroundTile;

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"Entered SceneDetails trigger: {name}");

            LoadScene();
            GameController.I.SetCurrentScene(this);

            // Load all connected scenes
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            // Unload any unconnected scenes
            var prevScene = GameController.I.PreviousScene;
            if (prevScene != null)
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }
                }

                if (!connectedScenes.Contains(prevScene))
                    prevScene.UnloadScene();
            }
        }
    }

    public SceneEntryPoint GetEntryPoint(string id)
    {
        return FindObjectsByType<SceneEntryPoint>(FindObjectsSortMode.None)
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
#endif

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsByType<SavableEntity>(FindObjectsSortMode.None).Where(x => x.gameObject.scene == currScene).ToList();
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

}
