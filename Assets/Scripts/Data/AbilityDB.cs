using System.Collections.Generic;

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
                OnModifyDef = (float spd, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    var result = spd;
                    if(defender.Status != null)
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

                    foreach (var stat in boosts.Keys)
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts.Remove(stat);
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

                    foreach (var stat in boosts.Keys)
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts.Remove(stat);
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
    };
}

public enum AbilityID { none, blaze, overgrow, torrent, swarm, guts, marvelscale, quickfeet, compoundeyes, keeneyes, hypercutter, bigpecks, clearbody, whitesmoke }
