using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
	[SerializeField] new string name;
	[TextArea]
	[SerializeField] string description;
	[SerializeField] Sprite icon;
	[SerializeField] int price = 100;
	[SerializeField] bool canSell = true;

	public virtual string Name => name;
	public string Desc => description;
	public Sprite Icon => icon;

	public virtual bool CanUseInBattle => true;
	public virtual bool CanUseOutOfBattle => true;
	public virtual bool IsReusable => false;

	public int SellPrice => price;
	public bool CanSell => canSell;

	public virtual bool Use(Pokemon target)
	{
		return false;
	}
}
