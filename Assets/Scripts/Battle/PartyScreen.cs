using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    List<PartyMemberUI> memberSlots;

    PokemonParty party;
    List<Pokemon> partyMembers;
    /// <summary>
    /// The BattleState from which the party screen was called.
    /// </br>Can be ActionSelection, RunningTurn, AboutToUse, etc.
    /// </summary>
    public BattleState? CalledFrom { get; set; }
    int selection = 0;

    public Pokemon SelectedPokemon => partyMembers[selection];

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true).ToList();
        party = PokemonParty.GetPlayerParty();

        party.OnUpdated += SetPartyData;
    }

    public void Cleanup()
    {
        party.OnUpdated -= SetPartyData;
    }

    public void SetPartyData()
    {
        partyMembers = party.Party;

        for (int i = 0; i < memberSlots.Count; i++)
        {
            if (i < partyMembers.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(partyMembers[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Pokemon";
    }

    public void HandleUpdate(Action OnSelected, Action OnBack)
    {
        int prevSelection = selection;

        MenuSelectionMethods.HandleGridSelection(ref selection, partyMembers.Count);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBack?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSelected?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selection)
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            memberSlots[i].SetSelected(i == selection);
        }
    }

    public void ShowIfTMIsUseable(TMItem tmItem)
    {
		for (int i = 0; i < party.Party.Count; i++)
        {
			Pokemon pokemon = party.Party[i];
            string message = tmItem.CanBeTaught(pokemon) ? "ABLE" : "UNABLE";
            message = pokemon.HasMove(tmItem.Move) ? "LEARNED" : message;
			memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMessages()
    {
		for (int i = 0; i < party.Party.Count; i++)
        {			
			memberSlots[i].SetMessage("");
        }
    }
    
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
