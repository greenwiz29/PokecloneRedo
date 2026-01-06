using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneDetails))]
[CanEditMultipleObjects]
public class SceneDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open Scene"))
            {
                foreach (var t in targets)
                {
                    var sceneDetails = t as SceneDetails;
                    if (sceneDetails != null)
                    {
                        sceneDetails.OpenSceneInEditor();
                        ValidateScene(sceneDetails);
                    }
                }
            }
            if (GUILayout.Button("Close Scene"))
            {
                foreach (var t in targets)
                {
                    var sceneDetails = t as SceneDetails;
                    if (sceneDetails != null)
                    {
                        sceneDetails.CloseSceneInEditor();
                        ValidateScene(sceneDetails);
                    }
                }
            }
        }
        base.OnInspectorGUI();
    }

    private void ValidateScene(SceneDetails sceneDetails)
    {
        string sceneName = sceneDetails.SceneName;

        bool inBuild = EditorBuildSettings.scenes
            .Any(s => System.IO.Path.GetFileNameWithoutExtension(s.path) == sceneName);

        if (!inBuild)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Scene '{sceneName}' is not in Build Settings.",
                MessageType.Warning
            );

            if (GUILayout.Button("Add to Build Settings"))
            {
                AddScene(sceneName);
            }
        }
    }

    void AddScene(string sceneName)
    {
        string path = AssetDatabase.FindAssets($"t:Scene {sceneName}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(path))
            return;

        var scenes = EditorBuildSettings.scenes.ToList();
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
