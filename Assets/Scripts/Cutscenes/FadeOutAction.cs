using System.Collections;
using UnityEngine;

public class FadeOutAction : CutsceneAction
{
    [SerializeField] float fadeDuration = 1f;
    
    public override IEnumerator Play()
    {
        yield return Fader.I.FadeOut(fadeDuration);
    }
}
