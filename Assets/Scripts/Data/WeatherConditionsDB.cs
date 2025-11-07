using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherConditionsDB
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

    public static Dictionary<WeatherConditionID, WeatherCondition> Conditions = new Dictionary<WeatherConditionID, WeatherCondition>()
    {
        {
            WeatherConditionID.sandstorm,
            new WeatherCondition()
            {
                Name = "Sandstorm",
                StartMessage = "A sandstorm is raging",
                EffectMessage = "The sandstorm rages",
                EndMessage = "The sandstorm subsides",
                OnWeatherEffect = p => {
                    if(p.IsOfType(PokemonType.Ground) || p.IsOfType(PokemonType.Rock))
                        return;

                    p.ReduceHP(p.MaxHP / 16);
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is buffeted by the sandstorm!");
                }
            }
        },{
            WeatherConditionID.hail,
            new WeatherCondition()
            {
                Name = "Hail",
                StartMessage = "Hail starts falling",
                EffectMessage = "Hail continues to fall",
                EndMessage = "The hail subsides",
                OnWeatherEffect = p => {
                    if(p.IsOfType(PokemonType.Ice))
                        return;

                    p.ReduceHP(p.MaxHP / 16);
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is buffeted by the hail!");
                }
            }
        },
    };
}

public class WeatherCondition
{
    public string Name { get; set; }
    public WeatherConditionID Id { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }
    public string EndMessage { get; set; }
    public Action<Pokemon> OnWeatherEffect { get; set; }
    public Func<Move, float> OnDamageModify { get; set; }

}

public enum WeatherConditionID { none, sandstorm, hail, rain, harsh_sun, }