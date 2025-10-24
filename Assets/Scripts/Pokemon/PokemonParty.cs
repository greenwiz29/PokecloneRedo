using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> party;

    public event Action OnUpdated;

    public List<Pokemon> Party
    {
        get => party;
        set
        {
            party = value;
            OnUpdated?.Invoke();
        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return GameController.I.Player.GetComponent<PokemonParty>();
    }

    void Awake()
    {
        foreach (var pokemon in party)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon(List<Pokemon> excluded = null)
    {
        var healthPokemon = party.Where(p => p.HP > 0);
        if(excluded != null)
        {
            healthPokemon = healthPokemon.Where(p => !excluded.Contains(p));
        }
        return healthPokemon.FirstOrDefault();
    }
    
    public List<Pokemon> GetHealthyPokemon(int count)
    {
        return party.Where(p => p.HP > 0).Take(count).ToList();
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (party.Count < 6)
        {
            party.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            // TODO: transfer to PC
        }
    }

    public void HealParty()
    {
        Party.ForEach(p => p.Heal());

        OnUpdated?.Invoke();
    }
}
