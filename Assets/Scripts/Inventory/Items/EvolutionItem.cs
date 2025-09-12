using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Evolution Item")]
public class EvolutionItem : ItemBase
{
	public override bool CanUseInBattle => false;

	public override bool Use(Pokemon target)
	{
		return true;
	}
}
