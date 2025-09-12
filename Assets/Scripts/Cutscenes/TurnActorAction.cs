using System.Collections;
using UnityEngine;

public class TurnActorAction : CutsceneAction
{
    [SerializeField] CutsceneActor actor;
    [SerializeField] FacingDirection direction;

	public override IEnumerator Play()
	{
		actor.Character.Animator.SetFacingDirection(direction);
        yield return null;
	}
}
