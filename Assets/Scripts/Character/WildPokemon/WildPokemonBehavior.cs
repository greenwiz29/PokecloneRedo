using System.Collections;
using UnityEngine;

public abstract class WildPokemonBehavior : ScriptableObject
{
    public abstract WildPersonality Personality {get;}
    public abstract IEnumerator Run(WildPokemonController controller);
}
