using System.Collections;
using UnityEngine;

public class NpcInteractAction : CutsceneAction
{
    [SerializeField] NpcController npc;
    
    public override IEnumerator Play()
    {
        yield return npc.Interact(GameController.I.Player.transform);
    }
}
