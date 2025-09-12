using System.Collections;
using UnityEngine;

[System.Serializable]
public class CutsceneAction
{
    [SerializeField] string name;
    [SerializeField] bool waitForCompleteion = true;

    public string Name { get => name; set => name = value; }
    public bool WaitForCompleteion => waitForCompleteion;

    public virtual IEnumerator Play()
    {
        yield break;
    }
}
