using System.Linq;
using GDEUtils.UI;

public class ActionSelectionUI : SelectionUI<TextSlot>
{
    void Start()
    {
        SetSelectionSettings(SelectionMode.GRID, 2);
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
}
