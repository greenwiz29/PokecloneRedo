using UnityEngine;

public class BattleField
{
    public WeatherCondition Weather { get; private set; }

    public void SetWeather(WeatherConditionID id)
    {
        if (id == WeatherConditionID.none)
            Weather = null;
        else
            Weather = WeatherConditionsDB.Conditions[id];
    }
}
