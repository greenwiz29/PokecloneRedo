using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => true;

    public void OnPlayerTriggered(PlayerController player)
    {
        float encounterRate = player.EncounterRate;
        // Player is walking on grass. Check random encounter rate
        if (Random.Range(1f, 101f) <= encounterRate)
        {
            player.Character.Animator.IsMoving = false;
            GameController.I.StartBattle(BattleTrigger.LongGrass);
        }
    }
}
