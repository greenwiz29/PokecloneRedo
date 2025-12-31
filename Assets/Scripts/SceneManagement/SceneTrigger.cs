using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTrigger : MonoBehaviour
{
    public enum SceneTypeEnum { World, Interior }

    [Header("Scene")]
    [SerializeField] string sceneName;
    [SerializeField] SceneTypeEnum sceneType;
    [SerializeField] SceneDetails sceneDetails;

    [Header("Bounds")]
    [SerializeField] BoxCollider2D bounds;

    public string SceneName => sceneName;
    public SceneTypeEnum SceneType => sceneType;
    public Bounds WorldBounds => bounds.bounds;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (bounds == null)
            bounds = GetComponent<BoxCollider2D>();

        if (sceneDetails == null)
            sceneDetails = GetComponentInChildren<SceneDetails>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.bounds.center, bounds.bounds.size);
    }
#endif
}
