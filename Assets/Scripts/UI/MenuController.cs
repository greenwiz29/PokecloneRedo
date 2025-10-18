using System.Linq;
using GDEUtils.UI;

public class MenuController : SelectionUI<TextSlot>
{
	void Start()
	{
		SetItems(GetComponentsInChildren<TextSlot>().ToList());
	}
}
