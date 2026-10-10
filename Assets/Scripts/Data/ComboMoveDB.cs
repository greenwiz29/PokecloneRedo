using System.Linq;
using UnityEngine;

public class ComboMoveDB : ScriptableObjectsDB<ComboMoveBase>
{
    public static ComboMoveBase FindCombo(MoveBase a, MoveBase b)
    {
        return objects?.FirstOrDefault(c => c.Value.IsTriggeredBy(a, b)).Value;
    }
}
