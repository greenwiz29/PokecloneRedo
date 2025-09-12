using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Pokeball")]
public class PokeballItem : ItemBase
{
	[SerializeField] float catchRateModifier = 1;
	[SerializeField] bool isMaster = false;

	public float CatchRateModifier => catchRateModifier;
	public bool IsMaster => isMaster;
	public override bool CanUseOutOfBattle => false;

	public override bool Use(Pokemon target)
	{
		return true;
	}
}
