using System.Collections.Generic;
using GDEUtils.UI;
using UnityEngine;

public class PokemonStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;

    void Start()
    {
        SetItems(boxSlots);
        SetSelectionSettings(SelectionMode.GRID, 8);
    }
}
