using System.Collections;
using UnityEngine;

public class FadeInAction : CutsceneAction
{
    [SerializeField] float fadeDuration = 1f;
    
    public override IEnumerator Play()
    {
        yield return Fader.I.FadeIn(fadeDuration);
    }
}
