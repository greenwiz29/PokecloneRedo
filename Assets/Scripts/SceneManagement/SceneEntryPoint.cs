using UnityEngine;

public class SceneEntryPoint : MonoBehaviour
{
    [SerializeField] string entryId;
    public string EntryId => entryId;
    
#if UNITY_EDITOR
    public void SetEditorId(string id)
    {
        entryId = id;
    }
#endif

}
