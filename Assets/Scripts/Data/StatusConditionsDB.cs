using System.Collections.Generic;
using UnityEngine;

public class StatusConditionsDB
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

    public static Dictionary<StatusConditionID, StatusCondition> Conditions { get; set; } = new Dictionary<StatusConditionID, StatusCondition>()
    {
        {
            StatusConditionID.psn,
            new StatusCondition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                CatchBonus = 1.5f,
                OnAfterTurn = (target, user) =>
                {
                    target.ReduceHP(target.MaxHP / 8);
                    target.AddStatusEvent(StatusEventType.Damage, $"{target.Base.Name} is hurt by poison!");
                }
            }
        },
        {
            StatusConditionID.brn,
            new StatusCondition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                CatchBonus = 1.5f,
                OnAfterTurn = (target, user) =>
                {
                    target.ReduceHP(target.MaxHP / 16);
                    target.AddStatusEvent(StatusEventType.Damage, $"{target.Base.Name} is hurt by its burn!");
                }
            }
        },
        {
            StatusConditionID.par,
            new StatusCondition()
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
            StatusConditionID.frz,
            new StatusCondition()
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
            StatusConditionID.slp,
            new StatusCondition()
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
            StatusConditionID.confusion,
            new StatusCondition()
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
        },
        {
            StatusConditionID.seeded,
            new StatusCondition()
            {
                Name = "Seeded",
                StartMessage = "was seeded!",
                CatchBonus = 1f,
                OnStart = p =>
                {
                    if (p.IsOfType(PokemonType.Grass))
                    {
                        // Grass-types are immune to being seeded
                        return;
                    }

                },
                OnAfterTurn = (Pokemon target, Pokemon user) =>
                {
                    int damage = target.MaxHP/16;
                    target.ReduceHP(damage, true);
                    user.IncreaseHP(damage, true);
                    target.AddStatusEvent(StatusEventType.Text, $"{target.Name}'s health is sapped by Leech Seed.");
                }
            }
        }
    };

    public static float GetStatusBonus(StatusCondition condition)
    {
        if (condition == null) return 1f;
        return condition.CatchBonus;
    }
}

public enum StatusConditionID { none, psn, brn, par, slp, frz, confusion, seeded }
