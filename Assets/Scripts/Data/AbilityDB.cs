using System.Collections.Generic;

public class AbilityDB
{
    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
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
    };
}

public enum AbilityID { none, blaze, overgrow, torrent, swarm, guts, marvelscale, quickfeet, compoundeyes }
