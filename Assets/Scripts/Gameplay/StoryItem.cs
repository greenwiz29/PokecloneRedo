using System.Collections;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;
    // [SerializeField] Transform spawnPoint;

    public bool blocksMovement = false;

    public bool TriggerRepeatedly => false;

    public void OnPlayerTriggered(PlayerController player)
    {
        StartCoroutine(StoryItemTriggered(player));
    }

    IEnumerator StoryItemTriggered(PlayerController player)
    {
        yield return DialogManager.I.ShowDialog(dialog);
        // player.Character.SetPositionAndSnapToTile(SpawnPoint.position);
    }
}
