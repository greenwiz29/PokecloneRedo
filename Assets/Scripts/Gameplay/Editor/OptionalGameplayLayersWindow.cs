using UnityEditor;
using UnityEngine;

public class OptionalGameplayLayersWindow : EditorWindow
{
    bool[] selected;
    string[] layerNames;

    bool confirmed;

    public static bool Show(
        string title,
        string description,
        string[] options,
        out bool[] selectedOptions
    )
    {
        var window = CreateInstance<OptionalGameplayLayersWindow>();
        window.titleContent = new GUIContent(title);
        window.layerNames = options;
        window.selected = new bool[options.Length];

        window.position = new Rect(
            Screen.currentResolution.width / 2f - 200,
            Screen.currentResolution.height / 2f - 150,
            400,
            300
        );

        window.ShowModalUtility();

        selectedOptions = window.selected;
        return window.confirmed;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(
            "Optional Gameplay Layers",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "Select which optional gameplay tilemaps should be created for this scene:",
            EditorStyles.wordWrappedLabel
        );

        EditorGUILayout.Space();

        for (int i = 0; i < layerNames.Length; i++)
        {
            selected[i] = EditorGUILayout.ToggleLeft(
                layerNames[i],
                selected[i]
            );
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Cancel", GUILayout.Width(100)))
        {
            confirmed = false;
            Close();
        }

        if (GUILayout.Button("Create Scene", GUILayout.Width(120)))
        {
            confirmed = true;
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}
