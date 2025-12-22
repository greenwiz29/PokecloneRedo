using System.Collections;

public interface IWildPokemonBehavior
{
    WildPersonality Personality {get;}
    IEnumerator Run(WildPokemonController controller);
}
