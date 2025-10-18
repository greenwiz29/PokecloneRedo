using System;
using System.Collections;
using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public static ShopSellingState I { get; private set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    Inventory inventory;
    public override void Enter(GameController owner)
    {
        gc = owner;
        inventory = Inventory.GetPlayerInventory();

        StartCoroutine(StartSellingState());
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
    }

    private IEnumerator StartSellingState()
    {
        yield return gc.stateMachine.PushAndWait(InventoryState.I);

        var itemToSell = InventoryState.I.SelectedItem;
        if (itemToSell != null)
        {
            yield return SellItem(itemToSell);
            StartCoroutine(StartSellingState());
        }
        else
        {
        gc.stateMachine.Pop();
        }
    }

    IEnumerator SellItem(ItemBase item)
    {
        if (!item.CanSell)
        {
            yield return DialogManager.I.ShowDialogText("You can't sell this item.");
            yield break;
        }

        int itemCount = inventory.GetItemCount(item);
        int countToSell = 1;
        if (itemCount > 1)
        {
            yield return DialogManager.I.ShowDialogText($"How many would you like to sell?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.Show(itemCount, item.SellPrice, true, selectedCount =>
            {
                countToSell = selectedCount;
            }, () =>
            {
                countToSell = 0; // Cancelled
            });
        }
        if (countToSell == 0)
        {
            DialogManager.I.CloseDialog();
            yield break;
        }

        walletUI.Show();

        int sellPrice = Mathf.RoundToInt(item.SellPrice * GlobalSettings.I.SellFactor) * countToSell;
        int selectedChoice = 0;
        yield return DialogManager.I.ShowDialogText($"I can give you ${sellPrice} for that. Do you still want to sell {(countToSell > 1 ? "them" : "it")}?", waitForInput: false, choices: new List<string> { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0) // Yes
        {
            inventory.RemoveItem(item, countToSell);
            Wallet.I.AddMoney(sellPrice);
            yield return DialogManager.I.ShowDialogText($"Sold {item.Name} for ${sellPrice}.");
        }
        else // No
        {
            yield return DialogManager.I.ShowDialogText("Alright, maybe next time.");
        }

        walletUI.Close();
    }

}
