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

    [MenuItem("Tools/Scenes/Create Overworld Scene Here")]
    public static void CreateOverworldSceneHere()
    {
        CreateSceneInternal(SceneTypeEnum.World);
    }

    [MenuItem("Tools/Scenes/Create Interior Scene Here")]
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
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneTriggerPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("SceneTrigger prefab not found.");
            return;
        }

        var trigger = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        trigger.name = scene.name;

        trigger.transform.position =
            SceneView.lastActiveSceneView?.pivot ?? Vector3.zero;

        var triggerComponent = trigger.GetComponent<SceneTrigger>();
        SerializedObject so = new SerializedObject(triggerComponent);

        so.FindProperty("sceneName").stringValue = scene.name;
        so.FindProperty("sceneType").enumValueIndex = (int)sceneType;

        so.ApplyModifiedProperties();

        Selection.activeGameObject = trigger;
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

}
