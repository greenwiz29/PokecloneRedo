using UnityEngine;

public interface ITerritoryReactive
{
    void OnTerritoryThreat(WildPokemonController c, Transform threat);
}
