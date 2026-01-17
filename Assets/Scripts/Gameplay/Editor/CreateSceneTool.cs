using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static SceneTrigger;

// TODO: Location of new scene is not where the mouse click was.
// TODO: tilemap layers are created with an offset from the Grid prefab.
public static class CreateSceneTool
{
    const string SceneTriggerPrefabPath = "Assets/Game/Resources/SceneManagement/SceneTrigger.prefab";
    const string GridPrefabPath = "Assets/Game/Resources/SceneManagement/Grid.prefab";
    const string EssentialLoaderPrefabPath = "Assets/Game/Resources/Main/EssentialObjectsLoader.prefab";
    const string CameraBoundsPrefabPath = "Assets/Game/Resources/Main/CameraBounds.prefab";

    #region Gameplay Tilemap Layer Definitions

    struct GameplayTilemapLayer
    {
        public string name;
        public string unityLayer;
    }

    // Always present
    static readonly GameplayTilemapLayer[] MandatoryGameplayLayers =
    {
        new() {
            name = "SolidObjects",
            unityLayer = "SolidObjects"
        }
    };

    // Optional
    static readonly GameplayTilemapLayer[] OptionalGameplayLayers =
    {
        new() {
            name = "LongGrass",
            unityLayer = "LongGrass"
        },
        new() {
            name = "Water",
            unityLayer = "Water"
        }
    };

    #endregion

    #region Menu Items

    [MenuItem("GameObject/Scenes/Create Overworld Scene Here")]
    public static void CreateOverworldSceneHere()
    {
        CreateSceneInternal(SceneTypeEnum.World);
    }

    [MenuItem("GameObject/Scenes/Create Interior Scene Here")]
    public static void CreateInteriorSceneHere()
    {
        CreateSceneInternal(SceneTypeEnum.Interior);
    }

    #endregion

    #region Core Flow

    static void CreateSceneInternal(SceneTypeEnum sceneType)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Scene",
            sceneType == SceneTypeEnum.Interior ? "NewInterior" : "NewScene",
            "unity",
            "Choose where to save the scene"
        );

        if (string.IsNullOrEmpty(path))
            return;

        Scene scene = CreateBaseScene(path);
        AddSceneToBuildSettingsIfMissing(scene.path);

        // Prompt for optional gameplay layers
        var optionalLayers = PromptForOptionalGameplayLayers();

        // Always present
        var grid = InstantiateInScene(GridPrefabPath, scene, "Grid");
        InstantiateInScene(EssentialLoaderPrefabPath, scene, "EssentialObjectsLoader");

        // Add gameplay tilemaps under Grid
        SetupGameplayTilemaps(grid, optionalLayers);

        // Interior extras
        if (sceneType == SceneTypeEnum.Interior)
        {
            InstantiateInScene(CameraBoundsPrefabPath, scene, "CameraBounds");
        }

        // SceneTrigger lives in Gameplay scene
        CreateSceneTrigger(scene, sceneType);

        Debug.Log($"{sceneType} scene '{scene.name}' created.");
    }

    static Scene CreateBaseScene(string path)
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Additive
        );

        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.SetActiveScene(scene);
        return scene;
    }

    #endregion

    #region Gameplay Tilemap Setup

    static List<GameplayTilemapLayer> PromptForOptionalGameplayLayers()
    {
        var selected = new List<GameplayTilemapLayer>();

        foreach (var layer in OptionalGameplayLayers)
        {
            if (EditorUtility.DisplayDialog(
                "Optional Gameplay Layers",
                $"Include '{layer.name}' tilemap?",
                "Include",
                "Skip"
            ))
            {
                selected.Add(layer);
            }
        }

        return selected;
    }

    static void SetupGameplayTilemaps(
        GameObject gridRoot,
        IEnumerable<GameplayTilemapLayer> optionalLayers
    )
    {
        foreach (var layer in MandatoryGameplayLayers)
            CreateGameplayTilemap(gridRoot.transform, layer);

        foreach (var layer in optionalLayers)
            CreateGameplayTilemap(gridRoot.transform, layer);
    }

    static void CreateGameplayTilemap(
        Transform gridRoot,
        GameplayTilemapLayer layer
    )
    {
        var go = new GameObject(layer.name);
        go.transform.SetParent(gridRoot);
        go.transform.localPosition = Vector3.zero;

        go.AddComponent<Tilemap>();

        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = "Foreground";

        int unityLayer = LayerMask.NameToLayer(layer.unityLayer);
        if (unityLayer == -1)
        {
            Debug.LogError($"Unity layer '{layer.unityLayer}' does not exist.");
        }
        else
        {
            go.layer = unityLayer;
        }
    }

    #endregion

    #region SceneTrigger Creation

    static void CreateSceneTrigger(Scene scene, SceneTypeEnum sceneType)
    {
        var gameplayScene = GetGameplayScene();
        if (!gameplayScene.IsValid())
            return;

        if (!gameplayScene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "Gameplay Scene Not Loaded",
                "Please open the Gameplay scene before creating new scenes.",
                "OK"
            );
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneTriggerPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("SceneTrigger prefab not found.");
            return;
        }

        var triggerGO = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        triggerGO.name = scene.name;

        SceneManager.MoveGameObjectToScene(triggerGO, gameplayScene);

        triggerGO.transform.position =
            SceneView.lastActiveSceneView?.pivot ?? Vector3.zero;

        var triggerComponent = triggerGO.GetComponent<SceneTrigger>();
        SerializedObject so = new(triggerComponent);

        so.FindProperty("sceneName").stringValue = scene.name;
        so.FindProperty("sceneType").enumValueIndex = (int)sceneType;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(gameplayScene);
        Selection.activeGameObject = triggerGO;
    }

    #endregion

    #region Utilities

    static GameObject InstantiateInScene(string prefabPath, Scene scene, string nameOverride)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            return null;

        var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.name = nameOverride;
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    static void AddSceneToBuildSettingsIfMissing(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();

        if (scenes.Any(s => s.path == scenePath))
            return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static Scene GetGameplayScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == "Gameplay")
                return s;
        }

        Debug.LogError("Gameplay scene must be loaded to create SceneTrigger.");
        return default;
    }

    #endregion
}
