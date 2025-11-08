using UnityEngine;

public class BattleField
{
    public WeatherCondition Weather { get; private set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(WeatherConditionID id, int? duration = null)
    {
        if (id == WeatherConditionID.none)
            Weather = null;
        else
            Weather = WeatherConditionsDB.Conditions[id];

        WeatherDuration = duration;
    }
}
