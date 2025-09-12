using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CutsceneActor))]
public class CutsceneActorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        var toggleWidth = 70;
        var togglePos = new Rect(position.x, position.y, toggleWidth, position.height);
        var fieldPos = new Rect(position.x + toggleWidth, position.y, position.width - toggleWidth, position.height);

        var isPlayerProp = property.FindPropertyRelative("isPlayer");
        isPlayerProp.boolValue = GUI.Toggle(togglePos, isPlayerProp.boolValue, "Is Player");
        isPlayerProp.serializedObject.ApplyModifiedProperties();

        if (!isPlayerProp.boolValue)
        {
            var characterProp = property.FindPropertyRelative("character");
            EditorGUI.PropertyField(fieldPos, characterProp, GUIContent.none);
        }
        EditorGUI.EndProperty();
    }
}
