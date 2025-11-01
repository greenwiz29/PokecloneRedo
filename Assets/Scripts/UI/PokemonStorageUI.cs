using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokemonStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;
    [SerializeField] Image movingPokemonImage;
    [SerializeField] float movingImageOffset = 50f;
    [SerializeField] TMP_Text boxNameText;

    List<BoxPartySlotUI> partySlots = new List<BoxPartySlotUI>();
    List<BoxSlotUI> boxSlotsUI = new List<BoxSlotUI>();
    List<Image> boxSlotImages = new List<Image>();

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
        boxSlotImages = boxSlots.Select(
            b => b.transform.GetChild(0).GetComponent<Image>()).ToList();
        movingPokemonImage.gameObject.SetActive(false);
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

    public override void HandleUpdate()
    {
        int prevBox = SelectedBox;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectedBox--;
            if (SelectedBox < 0)
                SelectedBox = storageBoxes.MaxBoxes - 1;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SelectedBox++;
            if (SelectedBox == storageBoxes.MaxBoxes)
                SelectedBox = 0;
        }

        if (SelectedBox != prevBox)
        {
            boxNameText.text = $"Box {SelectedBox + 1}";
            SetDataInStorageSlots();
            UpdateSelectionUI();
            return;
        }

        base.HandleUpdate();
    }

    public override void UpdateSelectionUI()
    {
        base.UpdateSelectionUI();

        if (movingPokemonImage.gameObject.activeSelf)
        {
            movingPokemonImage.transform.position =
                boxSlotImages[selection].transform.position + Vector3.up * movingImageOffset;
        }
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

        movingPokemonImage.gameObject.SetActive(false);
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
            if (pokemon == null)
                return null;

            party.Pokemon[partyIndex] = null;
        }
        else
        {
            int boxSlotIndex = slotIndex - (slotIndex / columns + 1);
            pokemon = storageBoxes.GetPokemon(SelectedBox, boxSlotIndex);
            if (pokemon == null)
                return null;

            storageBoxes.RemovePokemon(SelectedBox, boxSlotIndex);
        }

        movingPokemonImage.sprite = boxSlotImages[slotIndex].sprite;
        movingPokemonImage.transform.position =
            boxSlotImages[slotIndex].transform.position + Vector3.up * movingImageOffset;
        boxSlotImages[slotIndex].color = GlobalSettings.I.Transparent;
        movingPokemonImage.gameObject.SetActive(true);

        return pokemon;
    }
}
