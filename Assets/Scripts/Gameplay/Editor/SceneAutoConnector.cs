#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public class SceneAutoConnector : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Auto Connect Scenes")]
    void AutoConnect()
    {
        var anchors = FindObjectsByType<SceneTrigger>();

        foreach (var a in anchors)
        {
            var details = a.GetComponentInChildren<SceneDetails>();
            details.ConnectedScenes.Clear();

            foreach (var b in anchors)
            {
                if (a == b) continue;
                
                if (a.SceneType != SceneTrigger.SceneTypeEnum.World 
                    || b.SceneType != SceneTrigger.SceneTypeEnum.World)
                    continue;

                if (a.WorldBounds.Intersects(b.WorldBounds))
                {
                    var otherDetails = b.GetComponentInChildren<SceneDetails>();
                    details.ConnectedScenes.Add(otherDetails);
                }
            }

            EditorUtility.SetDirty(details);
        }
    }
#endif
}
