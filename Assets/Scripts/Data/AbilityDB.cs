using System;
using System.Collections.Generic;
using System.Linq;

public class AbilityDB
{
    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
        // Type-specific attack-boosting abilities
        {
            AbilityID.blaze,
            new Ability()
            {
                Name = "Blaze",
                Description = "Powers up Fire type moves when the Pokemon's HP is low",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = atk;
                    if(move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                },
                OnModifySpAtk = (float spAtk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spAtk;
                    if(move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.overgrow,
            new Ability()
            {
                Name = "Overgrow",
                Description = "Powers up Grass type moves when the Pokemon's HP is low",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = atk;
                    if(move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                },
                OnModifySpAtk = (float spAtk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spAtk;
                    if(move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.torrent,
            new Ability()
            {
                Name = "Torrent",
                Description = "Powers up Water type moves when the Pokemon's HP is low",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = atk;
                    if(move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                },
                OnModifySpAtk = (float spAtk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spAtk;
                    if(move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.swarm,
            new Ability()
            {
                Name = "Swarm",
                Description = "Powers up Bug type moves when the Pokemon's HP is low",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = atk;
                    if(move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                },
                OnModifySpAtk = (float spAtk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spAtk;
                    if(move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHP / 3)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        // Stat-boosting abilities
        {
            AbilityID.guts,
            new Ability()
            {
                Name = "Guts",
                Description = "Boosts Attack if there is a status condition",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = atk;
                    if(attacker.Status != null)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.marvelscale,
            new Ability()
            {
                Name = "Marvel Scale",
                Description = "Boosts Defense if there is a status condition",
                OnModifyDef = (float def, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = def;
                    if(defender.Status != null)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.quickfeet,
            new Ability()
            {
                Name = "Quick Feet",
                Description = "Boosts Speed if there is a status condition",
                OnModifySpd = (float spd, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spd;
                    if(attacker.Status != null)
                    {
                        result *= 1.5f;
                    }
                    return result;
                }
            }
        },
        {
            AbilityID.compoundeyes,
            new Ability()
            {
                Name = "Compound Eyes",
                Description = "Boosts Accuracy",
                OnModifyAcc = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    return acc * 1.3f;
                }
            }
        },
        // Stat-change blocking abilities
        {
            AbilityID.keeneyes,
            new Ability()
            {
                Name = "Keen Eyes",
                Description = "Prevents the Pokemon from loosing accuracy.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon source, Pokemon target) =>
                {
                    // self boost
                    if(source != null && source == target)
                        return;

                    if(boosts.ContainsKey(Stat.Accuracy) && boosts[Stat.Accuracy] < 0)
                    {
                        boosts.Remove(Stat.Accuracy);
                        target.AddStatusEvent(StatusEventType.Text, $"{target.Base.Name}'s accuracy can't be lowered thanks to its keen eyes.");
                    }
                }
            }
        },
        {
            AbilityID.hypercutter,
            new Ability()
            {
                Name = "Hyper Cutter",
                Description = "Prevents other Pokemon from lowering attack.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon source, Pokemon target) =>
                {
                    // self boost
                    if(source != null && source == target)
                        return;

                    if(boosts.ContainsKey(Stat.Attack) && boosts[Stat.Attack] < 0)
                    {
                        boosts.Remove(Stat.Attack);
                        target.AddStatusEvent(StatusEventType.Text, $"{target.Base.Name}'s attact can't be lowered.");
                    }
                }
            }
        },
        {
            AbilityID.bigpecks,
            new Ability()
            {
                Name = "Big Pecks",
                Description = "Prevents other Pokemon from lowering defense.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon source, Pokemon target) =>
                {
                    // self boost
                    if(source != null && source == target)
                        return;

                    if(boosts.ContainsKey(Stat.Defense) && boosts[Stat.Defense] < 0)
                    {
                        boosts.Remove(Stat.Defense);
                        target.AddStatusEvent(StatusEventType.Text, $"{target.Base.Name}'s defense can't be lowered.");
                    }
                }
            }
        },
        {
            AbilityID.clearbody,
            new Ability()
            {
                Name = "Clear Body",
                Description = "Prevents other Pokemon from lowering any stats.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon source, Pokemon target) =>
                {
                    bool boostRemoved = false;
                    // self boost
                    if(source != null && source == target)
                        return;

                    foreach (var stat in boosts.Keys.ToList())
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts[stat] = 0;
                            boostRemoved = true;
                        }
                    }
                    if (boostRemoved)
                    {
                        target.AddStatusEvent(StatusEventType.Text, $"{target.Base.Name}'s clear body prevents stat loss.");
                    }
                }
            }
        },
        {
            AbilityID.whitesmoke,
            new Ability()
            {
                Name = "White Smoke",
                Description = "Prevents other Pokemon from lowering any stats.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon source, Pokemon target) =>
                {
                    bool boostRemoved = false;
                    // self boost
                    if(source != null && source == target)
                        return;

                    foreach (var stat in boosts.Keys.ToList())
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts[stat] = 0;
                            boostRemoved = true;
                        }
                    }
                    if (boostRemoved)
                    {
                        target.AddStatusEvent(StatusEventType.Text, $"{target.Base.Name}'s white smoke prevents stat loss.");
                    }
                }
            }
        },
        // Prevent status conditions
        {
            AbilityID.insomnia,
            new Ability()
            {
                Name = "Insomnia",
                Description = "Prevents the Pokemon from falling asleep.",
                OnTrySetStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.slp)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name}'s insomnia won't let it fall asleep...");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        {
            AbilityID.immunity,
            new Ability()
            {
                Name = "Poison Immunity",
                Description = "Prevents the Pokemon from being poisoned.",
                OnTrySetStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.psn)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name} is immune to poison.");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        {
            AbilityID.limber,
            new Ability()
            {
                Name = "Limber",
                Description = "Prevents the Pokemon from being paralyzed.",
                OnTrySetStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.par)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name} is too flexible to be paralyzed.");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        {
            AbilityID.waterveil,
            new Ability()
            {
                Name = "Water Veil",
                Description = "Prevents the Pokemon from being burned.",
                OnTrySetStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.brn)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name}'s watery physique prevents burns.");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        {
            AbilityID.vitalspirit,
            new Ability()
            {
                Name = "Vital Spirit",
                Description = "Prevents the Pokemon from falling asleep.",
                OnTrySetStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.slp)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name}'s fighting spirit won't let it fall asleep...");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        {
            AbilityID.owntempo,
            new Ability()
            {
                Name = "Own Tempo",
                Description = "Prevents the Pokemon from becoming confused.",
                OnTrySetVolatileStatus = (StatusConditionID condition, Pokemon p, EffectSource source) =>
                {
                    if(condition == StatusConditionID.confusion)
                    {
                        if(source == EffectSource.Move)
                        {
                            p.AddStatusEvent(StatusEventType.Text, $"{p.Name} moves at its own tempo...");
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        },
        // inflict status condition on contact
        {
            AbilityID.staticbody,
            new Ability()
            {
                Name = "Static Body",
                Description = "Contact with this Pokemon may cause paralysis",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && UnityEngine.Random.Range(1, 101) <- 30)
                    {
                        attacker.SetStatus(StatusConditionID.par, EffectSource.Ability);
                    }
                }
            }
        },
        {
            AbilityID.poisonpoint,
            new Ability()
            {
                Name = "Poison Point",
                Description = "Contact with this Pokemon may inflict poison",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && UnityEngine.Random.Range(1, 101) <- 30)
                    {
                        attacker.SetStatus(StatusConditionID.psn, EffectSource.Ability);
                    }
                }
            }
        },
        {
            AbilityID.flamebody,
            new Ability()
            {
                Name = "Flame Body",
                Description = "Contact with this Pokemon may inflict burns",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && UnityEngine.Random.Range(1, 101) <- 30)
                    {
                        attacker.SetStatus(StatusConditionID.brn, EffectSource.Ability);
                    }
                }
            }
        },
    };
}

public enum AbilityID { none, 
blaze, overgrow, torrent, swarm, guts, marvelscale, quickfeet, compoundeyes, 
keeneyes, hypercutter, bigpecks, clearbody, whitesmoke, 
insomnia, immunity, limber, waterveil, vitalspirit, owntempo,
staticbody, poisonpoint,flamebody, }
