using System.Collections;
using UnityEngine;

public class EnableGameObjectAction : CutsceneAction
{
    [SerializeField] GameObject gameObject;
    public override IEnumerator Play()
    {
        gameObject.SetActive(true);
        yield break;
    }
}
