using System.Collections;
using UnityEngine;

public class GymLeaderController : TrainerController
{
    [Header("Gym Rewards")]
    [SerializeField] ItemBase badge;
    [SerializeField] ItemBase rewardItem;
    [SerializeField] int rewardItemCount = 1;

    [Header("Quest Progression")]
    [SerializeField] Quest questToStart;
    [SerializeField] Quest questToComplete;
    [SerializeField] WorldStateFlag gymClearedFlag;

    protected override bool CanTriggerFieldBattle => false;

    public override IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!IsBattleLost)
        {
            questToStart?.StartQuest();

            yield return DialogManager.I.ShowDialog(preBattleDialog);
            GameController.I.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogManager.I.ShowDialog(postBattleDialog);
        }
    }

    public override void BattleLost()
    {
        base.BattleLost();

        if (badge != null)
            Inventory.GetPlayerInventory().AddItem(badge, 1);

        if (rewardItem != null)
            Inventory.GetPlayerInventory().AddItem(rewardItem, rewardItemCount);

        questToComplete?.CompleteQuest();
    }
}
