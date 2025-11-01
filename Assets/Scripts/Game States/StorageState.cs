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
    public override void Enter(GameController owner)
    {
        gc = owner;
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
        
    }

    private void OnBack()
    {
        gc.stateMachine.Pop();
    }

}
