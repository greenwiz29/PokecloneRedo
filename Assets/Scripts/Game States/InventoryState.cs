using System;
using GDEUtils.StateMachine;
using UnityEngine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;

    public static InventoryState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OnSelected += OnItemSelected;
        inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.OnSelected -= OnItemSelected;
        inventoryUI.OnBack -= OnBack;
    }

    private void OnBack()
    {
        gc.stateMachine.Pop();
    }

    private void OnItemSelected(int obj)
    {
        if (inventoryUI.Category == ItemCategories.KEY)
            return; // For now, key items can't be used directly
        else
            gc.stateMachine.Push(GamePartyState.I);
    }
}
