using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] TMP_Text messageText;
    List<PartyMemberUI> memberSlots;

    PokemonParty party;
    List<Pokemon> partyMembers;
    
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

        var textSlots = memberSlots.Select(m => m.GetComponent<TextSlot>()).ToList();
        SetItems(textSlots.Take(partyMembers.Count).ToList());

        messageText.text = "Choose a Pokemon";
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
