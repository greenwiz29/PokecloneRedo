using System.Collections;
using UnityEngine;

public class DisableGameObjectAction : CutsceneAction
{
    [SerializeField] GameObject gameObject;

    public override IEnumerator Play()
    {
        gameObject.SetActive(false);
        yield break;
    }
}
