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
    int columns = 8;
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
        SetSelectionSettings(SelectionMode.GRID, columns);
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
                partySlots[i].ClearData();
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
                boxSlotsUI[i].ClearData();
        }
    }

    public bool IsPartySlot(int slotIndex)
    {
        return slotIndex % columns == 0;
    }

    public void PutPokemonInSlot(Pokemon pokemon, int slotIndex)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / columns;
            if (partyIndex >= party.Pokemon.Count)
            {
                party.Pokemon.Add(pokemon);
            }
            else
            {
                party.Pokemon[partyIndex] = pokemon;
            }
        }
        else
        {
            int boxSlotIndex = slotIndex - (slotIndex / columns + 1);
            storageBoxes.AddPokemon(pokemon, SelectedBox, boxSlotIndex);
        }
    }

    public Pokemon TakePokemonFromSlot(int slotIndex)
    {
        Pokemon pokemon;
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / columns;
            if (partyIndex >= party.Pokemon.Count)
            {
                return null;
            }

            pokemon = party.Pokemon[partyIndex];
            party.Pokemon[partyIndex] = null;
        }
        else
        {
            int boxSlotIndex = slotIndex - (slotIndex / columns + 1);
            pokemon = storageBoxes.GetPokemon(SelectedBox, boxSlotIndex);
            storageBoxes.RemovePokemon(SelectedBox, boxSlotIndex);
        }
        return pokemon;
    }
}
