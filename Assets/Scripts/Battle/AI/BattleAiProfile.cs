using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Profile")]
public class BattleAIProfile : ScriptableObject
{
    public List<AIRule> rules;
}
