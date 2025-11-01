using System;
using GDEUtils.StateMachine;
using UnityEngine;

public class StorageState : State<GameController>
{
    [SerializeField] PokemonStorageUI storageUI;
    public static StorageState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    GameController gc;
    bool isMovingPokemon = false;
    int selectedSlotToMove = 0;
    Pokemon pokemonToMove;
    PokemonParty party;

    public override void Enter(GameController owner)
    {
        gc = owner;
        party = PokemonParty.GetPlayerParty();
        storageUI.gameObject.SetActive(true);
        storageUI.SetDataInPartySlots();
        storageUI.SetDataInStorageSlots();
        storageUI.OnBack += OnBack;
        storageUI.OnSelected += OnSlotSelected;
    }

    public override void Execute()
    {
        storageUI.HandleUpdate();
    }

    public override void Exit()
    {
        storageUI.gameObject.SetActive(false);
        storageUI.OnBack -= OnBack;
        storageUI.OnSelected -= OnSlotSelected;
    }

    private void OnSlotSelected(int selection)
    {
        if (!isMovingPokemon)
        {
            var pokemon = storageUI.TakePokemonFromSlot(selection);
            if (pokemon != null)
            {
                isMovingPokemon = true;
                selectedSlotToMove = selection;
                pokemonToMove = pokemon;
            }
        }
        else
        {
            isMovingPokemon = false;

            int firstSlotIndex = selectedSlotToMove;
            int secondSlotIndex = selection;

            var secondPokemon = storageUI.TakePokemonFromSlot(secondSlotIndex);

            if (secondPokemon == null && storageUI.IsPartySlot(firstSlotIndex)
                && storageUI.IsPartySlot(secondSlotIndex))
            {
                storageUI.PutPokemonInSlot(pokemonToMove, firstSlotIndex);
                return;
            }
            
            storageUI.PutPokemonInSlot(pokemonToMove, secondSlotIndex);
            if (secondPokemon != null)
                storageUI.PutPokemonInSlot(secondPokemon, firstSlotIndex);

            party.Pokemon.RemoveAll(p => p == null);
            party.PartyUpdated();

            storageUI.SetDataInPartySlots();
            storageUI.SetDataInStorageSlots();
        }
    }

    private void OnBack()
    {
        if (isMovingPokemon)
        {
            isMovingPokemon = false;
            storageUI.PutPokemonInSlot(pokemonToMove, selectedSlotToMove);
        }
        else
            gc.stateMachine.Pop();
    }

}
