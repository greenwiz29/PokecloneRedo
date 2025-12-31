using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateSceneTool
{
    [MenuItem("Tools/Scenes/Create Scene Here")]
    public static void CreateSceneHere()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Scene",
            "NewScene",
            "unity",
            "Choose where to save the new scene"
        );

        if (string.IsNullOrEmpty(path))
            return;

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Additive
        );

        EditorSceneManager.SaveScene(scene, path);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/SceneAnchor.prefab"
        );

        var anchor = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        anchor.name = scene.name;

        Vector3 position = SceneView.lastActiveSceneView?.pivot ?? Vector3.zero;
        anchor.transform.position = position;

        var anchorComponent = anchor.GetComponent<SceneTrigger>();
        SerializedObject so = new SerializedObject(anchorComponent);
        so.FindProperty("sceneName").stringValue = scene.name;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = anchor;
    }

}
