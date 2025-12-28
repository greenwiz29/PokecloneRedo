using System.Collections.Generic;

public enum BattleActionType { Move, Switch, Item, Run, }

public class BattleAction
{
    public BattleActionType Type { get; set; }
    public BattleUnit User { get; set; }
    // For single-target moves
    public BattleUnit Target { get; set; }
    // For multi-target moves (Area, AllFoes, etc)
    public List<BattleUnit> Targets { get; set; }

    // Optional but useful
    public MoveTarget MoveTargetType { get; set; }

    public Move SelectedMove { get; set; }
    public Pokemon SelectedPokemon { get; set; }
    public ItemBase SelectedItem { get; set; }
    public int Priority => Type == BattleActionType.Move ? SelectedMove?.Base.Priority ?? 0 : 99;

    public bool IsInvalid { get; set; }
}
