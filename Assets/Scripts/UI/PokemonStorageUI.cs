using System.Collections.Generic;
using GDEUtils.UI;
using UnityEngine;

public class PokemonStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;

    List<BoxPartySlotUI> partySlots = new List<BoxPartySlotUI>();
    List<BoxSlotUI> boxSlotsUI = new List<BoxSlotUI>();

    PokemonParty party;
    PokemonStorageBox storageBoxes;
    public int SelectedBox { get; private set; } = 0;
    void Awake()
    {
        foreach (var slot in boxSlots)
        {
            var boxSlot = slot.GetComponent<BoxSlotUI>();
            if (boxSlot != null)
            {
                boxSlotsUI.Add(boxSlot);
            }
            else
            {
                partySlots.Add(slot.GetComponent<BoxPartySlotUI>());
            }
        }
        party = PokemonParty.GetPlayerParty();
        storageBoxes = PokemonStorageBox.GetPlayerStorageBoxes();
    }

    void Start()
    {
        if (didStart)
            return;

        if (!didAwake)
            Awake();

        SetItems(boxSlots);
        SetSelectionSettings(SelectionMode.GRID, 8);
    }

    public void SetDataInPartySlots()
    {
        if (!didStart)
            Start();

        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < party.Pokemon.Count)
            {
                partySlots[i].SetData(party.Pokemon[i]);
            }
            else
            {
                partySlots[i].Clear();
            }
        }
    }

    public void SetDataInStorageSlots()
    {
        for (int i = 0; i < boxSlotsUI.Count; i++)
        {
            var pokemon = storageBoxes.GetPokemon(SelectedBox, i);
            if (pokemon != null)
            {
                boxSlotsUI[i].SetData(pokemon);
            }
            else
                boxSlotsUI[i].Clear();
        }
    }
}
