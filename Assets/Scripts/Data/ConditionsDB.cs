using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionID;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                CatchBonus = 1.5f,
                OnAfterTurn = p =>
                {
                    p.ReduceHP(p.MaxHP / 8);
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is hurt by poison!");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                CatchBonus = 1.5f,
                OnAfterTurn = p =>
                {
                    p.ReduceHP(p.MaxHP / 16);
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is hurt by its burn!");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed. It may not be able to move!",
                CatchBonus = 1.5f,
                OnBeforeMove = p =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        p.AddStatusEvent($"{p.Base.Name} is paralyzed and can't move.");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen. It may not be able to move!",
                CatchBonus = 2f,
                OnBeforeMove = p =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        p.AddStatusEvent($"{p.Base.Name} is no longer frozen.");
                        p.CureStatus();
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep.",
                CatchBonus = 2f,
                OnStart = p =>
                {
                    // sleep for 1-3 turns
                    p.StatusTime = Random.Range(1, 4);

                },
                OnBeforeMove = p =>
                {
                    if (p.StatusTime <= 0)
                    {
                        p.CureStatus();
                        p.AddStatusEvent($"{p.Base.Name} woke up!");
                        return true;
                    }
                    p.StatusTime--;
                    p.AddStatusEvent($"{p.Base.Name} is sleeping.");
                    return false;
                }
            }
        },

        // Volatile conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has become confused.",
                CatchBonus = 1f,
                OnStart = p =>
                {
                    // confused for 1-4 turns
                    p.VolatileStatusTime = Random.Range(1, 5);

                },
                OnBeforeMove = p =>
                {
                    if (p.VolatileStatusTime <= 0)
                    {
                        p.CureVolatileStatus();
                        p.AddStatusEvent($"{p.Base.Name} came to its senses!");
                        return true;
                    }
                    p.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                    {
                        return true;
                    }

                    // Hurt by confusion
                    p.AddStatusEvent($"{p.Base.Name} is confused.");
                    p.ReduceHP(p.MaxHP / 8);
                    p.AddStatusEvent(StatusEventType.Damage, $"It hurt itself in its confusion.");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null) return 1f;
        return condition.CatchBonus;
    }
}

public enum ConditionID { none, psn, brn, par, slp, frz, confusion }
