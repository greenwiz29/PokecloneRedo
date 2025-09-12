using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TMItem : ItemBase
{
	[SerializeField] MoveBase move;

	public override string Name => base.Name + $": {move.Name}";
	public override bool CanUseInBattle => false;
	public override bool IsReusable => true;

	public MoveBase Move => move;

	public override bool Use(Pokemon target)
	{
		// Learning move is handled from InventoryUI.
		return target.HasMove(move);
	}

	public bool CanBeTaught(Pokemon pokemon)
	{
		return pokemon.Base.LearnableByItems.Contains(move);
	}
}
