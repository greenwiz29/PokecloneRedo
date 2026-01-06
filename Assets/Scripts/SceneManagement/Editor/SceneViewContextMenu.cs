#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneViewContextMenus
{
    static SceneViewContextMenus()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // RIGHT MOUSE BUTTON
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(
                new GUIContent("Scenes/Create Overworld Scene Here"),
                false,
                () => CreateSceneTool.CreateOverworldSceneHere()
            );

            menu.AddItem(
                new GUIContent("Scenes/Create Interior Scene Here"),
                false,
                () => CreateSceneTool.CreateInteriorSceneHere()
            );

            menu.ShowAsContext();

            // 🔑 THIS IS CRITICAL
            e.Use();
        }
    }
}
#endif
