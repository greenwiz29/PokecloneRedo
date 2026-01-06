using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneTrigger;

public static class CreateSceneTool
{
    const string SceneTriggerPrefabPath = "Assets/Game/Resources/SceneManagement/SceneTrigger.prefab";
    const string GridPrefabPath = "Assets/Game/Resources/SceneManagement/Grid.prefab";
    const string EssentialLoaderPrefabPath = "Assets/Game/Resources/Main/EssentialObjectsLoader.prefab";
    const string CameraBoundsPrefabPath = "Assets/Game/Resources/Main/CameraBounds.prefab";

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

        // Always present
        InstantiateInScene(GridPrefabPath, scene, "Grid");
        InstantiateInScene(EssentialLoaderPrefabPath, scene, "EssentialObjectsLoader");

        // Interior extras
        if (sceneType == SceneTypeEnum.Interior)
        {
            InstantiateInScene(CameraBoundsPrefabPath, scene, "CameraBounds");
        }

        // SceneTrigger lives in the layout/world scene
        CreateSceneTrigger(scene, sceneType);

        Debug.Log($"{sceneType} scene '{scene.name}' created.");
    }

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

        // 🔑 MOVE TO GAMEPLAY SCENE
        SceneManager.MoveGameObjectToScene(triggerGO, gameplayScene);

        triggerGO.transform.position =
            SceneView.lastActiveSceneView?.pivot ?? Vector3.zero;

        var triggerComponent = triggerGO.GetComponent<SceneTrigger>();
        SerializedObject so = new SerializedObject(triggerComponent);

        so.FindProperty("sceneName").stringValue = scene.name;
        so.FindProperty("sceneType").enumValueIndex = (int)sceneType;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(gameplayScene);
        Selection.activeGameObject = triggerGO;
    }

    static void InstantiateInScene(string prefabPath, Scene scene, string nameOverride)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            return;

        var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.name = nameOverride;
        SceneManager.MoveGameObjectToScene(go, scene);
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

}
