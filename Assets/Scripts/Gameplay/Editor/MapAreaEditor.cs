using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var totalChance = serializedObject.FindProperty("totalChance").intValue;

        var totalChanceWater = serializedObject.FindProperty("totalChanceWater").intValue;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };
        GUILayout.Label($"Total Encounter Rate: {totalChance}", style);

        if (totalChance != 100)
        {
            EditorGUILayout.HelpBox("Warning: Total encounter rate is not equal to 100%", MessageType.Error);
        }

        GUILayout.Label($"Total Encounter Rate in Water: {totalChanceWater}", style);

        if (totalChanceWater != 100)
        {
            EditorGUILayout.HelpBox("Warning: Total encounter rate in water is not equal to 100%", MessageType.Error);
        }
    }
}
