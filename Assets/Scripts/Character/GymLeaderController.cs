using System.Collections;
using UnityEngine;

public class GymLeaderController : TrainerController
{
    [Header("Gym Rewards")]
    [SerializeField] GymBadge badge;
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
        StartCoroutine(GiveRewardItems());
        
        questToStart?.StartQuest();
        questToComplete?.CompleteQuest();

        base.BattleLost();
    }

    IEnumerator GiveRewardItems()
    {
        yield return DialogManager.I.ShowDialog(postBattleDialog);

        if (badge != null)
        {
            Inventory.GetPlayerInventory().AddItem(badge, 1);
            yield return DialogManager.I.ShowDialogText($"You received {badge.Name} X{1}");
        }

        if (rewardItem != null)
        {
            yield return DialogManager.I.ShowDialogText($"Take this as well.");

            Inventory.GetPlayerInventory().AddItem(rewardItem, rewardItemCount);
            yield return DialogManager.I.ShowDialogText($"You received {rewardItem.Name} X{rewardItemCount}");
        }
    }
}
