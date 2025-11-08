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
                StartByMoveMessage = "A sandstorm starts raging",
                EffectMessage = "The sandstorm rages",
                EndMessage = "The sandstorm subsides",
                OnWeatherEffect = p => {
                    if(p.IsOfType(PokemonType.Ground) || p.IsOfType(PokemonType.Rock))
                        return;
                    p.ReduceHP(Mathf.CeilToInt(p.MaxHP / 16 * p.GetTypeEffectiveness(PokemonType.Ground)));
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is buffeted by the sandstorm!");
                }
            }
        },
        {
            WeatherConditionID.hail,
            new WeatherCondition()
            {
                Name = "Hail",
                StartMessage = "Hail is falling",
                StartByMoveMessage = "Hail starts falling",
                EffectMessage = "Hail continues to fall",
                EndMessage = "The hail stopped",
                OnWeatherEffect = p => {
                    if(p.IsOfType(PokemonType.Ice))
                        return;

                    p.ReduceHP(Mathf.CeilToInt(p.MaxHP / 16 * p.GetTypeEffectiveness(PokemonType.Ice)));
                    p.AddStatusEvent(StatusEventType.Damage, $"{p.Base.Name} is buffeted by the hail!");
                }
            }
        },
        {
            WeatherConditionID.rain,
            new WeatherCondition()
            {
                Name = "Rain",
                StartMessage = "Rain is falling",
                StartByMoveMessage = "Rain starts falling",
                EffectMessage = "Rain continues to fall",
                EndMessage = "The rain stopped",
                OnDamageModify = m =>
                {
                    if(m.Base.Type == PokemonType.Water)
                    {
                        return 1.5f;
                    }
                    else if (m.Base.Type == PokemonType.Fire)
                    {
                        return 0.5f;
                    }
                    else
                    {
                        return 1f;
                    }
                }
            }
        },
        {
            WeatherConditionID.harsh_sun,
            new WeatherCondition()
            {
                Name = "Harsh Sunlight",
                StartMessage = "The sunlight is harsh",
                StartByMoveMessage = "The sunlight becomes harsh",
                EffectMessage = "The sunlight is harsh",
                EndMessage = "The sunlight faded",
                OnDamageModify = m =>
                {
                    if(m.Base.Type == PokemonType.Water)
                    {
                        return 0.5f;
                    }
                    else if (m.Base.Type == PokemonType.Fire)
                    {
                        return 1.5f;
                    }
                    else
                    {
                        return 1f;
                    }
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
    public string StartByMoveMessage { get; set; }
    public string EffectMessage { get; set; }
    public string EndMessage { get; set; }
    public Action<Pokemon> OnWeatherEffect { get; set; }
    public Func<Move, float> OnDamageModify { get; set; }

}

public enum WeatherConditionID { none, sandstorm, hail, rain, harsh_sun, }