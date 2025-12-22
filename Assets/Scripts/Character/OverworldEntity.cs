using System.Collections;
using UnityEngine;

public abstract class OverworldEntity : MonoBehaviour, IInteractable
{
    protected Character character;
    protected NPCState state = NPCState.Idle;

    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }

    protected virtual void Update()
    {
        character.HandleUpdate();
    }

    protected abstract IEnumerator OnInteract(Transform initiator);

    public IEnumerator Interact(Transform initiator)
    {
        if (state != NPCState.Idle)
            yield break;

        state = NPCState.Dialog;
        yield return OnInteract(initiator);
        state = NPCState.Idle;
    }
}
