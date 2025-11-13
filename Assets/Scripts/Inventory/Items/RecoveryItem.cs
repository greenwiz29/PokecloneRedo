using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Recovery Item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpRestoredAmount;
    [SerializeField] bool restoreMaxHp;

    [Header("PP")]
    [SerializeField] int ppRestoredAmount;
    [SerializeField] bool restoreMaxPp;

    [Header("Status")]
    [SerializeField] StatusConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon target)
    {
        int hpRestoreAmt;

        if (revive || maxRevive)
        {
            if (target.HP > 0)
            {
                return false;
            }

            hpRestoreAmt = maxRevive ? target.MaxHP : target.MaxHP / 2;
            target.IncreaseHP(hpRestoreAmt, true);
            target.CureStatus();
            target.CureVolatileStatus();
            return true;
        }

        // Non-revive items won't work on a fainted 'mon
        if (target.HP <= 0)
            return false;

        if (hpRestoredAmount > 0 || restoreMaxHp)
        {
            if (target.HP == target.MaxHP)
            {
                // full health, it won't have any effect
                return false;
            }
            hpRestoreAmt = restoreMaxHp ? target.MaxHP : hpRestoredAmount;
            target.IncreaseHP(hpRestoreAmt, true);
            return true;
        }

        if (recoverAllStatus || status != StatusConditionID.none)
        {
            if (target.Status == null && target.VolatileStatus == null)
                return false;

            if (recoverAllStatus)
            {
                target.CureStatus();
                target.CureVolatileStatus();
            }
            if (target.Status.Id == status)
                target.CureStatus();
            else if (target.VolatileStatus.Id == status)
                target.CureVolatileStatus();
            else
                return false;

            return true;
        }

        // Restore PP to all moves
        if (restoreMaxPp)
        {
            target.Moves.ForEach(m => m.IncreasePP(m.MaxPP));
        }
        else if (ppRestoredAmount > 0)
        {
            target.Moves.ForEach(m => m.IncreasePP(ppRestoredAmount));
        }

        return false;
    }
}
