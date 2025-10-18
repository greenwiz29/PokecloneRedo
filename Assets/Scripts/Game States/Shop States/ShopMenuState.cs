using System.Collections;
using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

    /// <summary>
    /// Make sure to set <see cref="AvailableItems"/> before pushing this state!
    /// </summary>
public class ShopMenuState : State<GameController>
{
    /// <summary>
    /// Make sure to set <see cref="AvailableItems"/> before pushing this state!
    /// </summary>
    public static ShopMenuState I { get; private set; }

    // Input
    public List<ItemBase> AvailableItems { get; set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        StartCoroutine(StartMenuState());
    }

    IEnumerator StartMenuState()
    {
        int selectedChoice = 0;
        yield return DialogManager.I.ShowDialogText("How can I help you today?", waitForInput: false, choices: new List<string> { "Buy", "Sell", "Leave" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        switch (selectedChoice)
        {
            case 0: // Buy
                ShopBuyingState.I.AvailableItems = AvailableItems;
                yield return gc.stateMachine.PushAndWait(ShopBuyingState.I);
                break;
            case 1: // Sell
                yield return gc.stateMachine.PushAndWait(ShopSellingState.I);
                break;
            case 2: // Leave
                yield return DialogManager.I.ShowDialogText("Thank you for your business!");
                gc.stateMachine.Pop();
                yield break;
        }

        StartCoroutine(StartMenuState());
    }

}
