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
    public SceneDetails SceneDetails => sceneDetails;
    public Bounds WorldBounds => bounds.bounds;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameController.I.CurrentScene == sceneDetails)
                return;

            Debug.Log($"Entered SceneDetails trigger: {name}");

            GameController.I.TransitionToScene(sceneDetails);
        }
    }

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
