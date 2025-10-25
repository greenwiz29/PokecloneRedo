using System;
using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="MenuItems"/> before pushing this state!
/// </summary>
public class DynamicMenuState : State<GameController>
{
    [SerializeField] DynamicMenuUI dynamicMenuUI;
    [SerializeField] TextSlot itemTextPrefab;

    /// <summary>
    /// Make sure to set <see cref="MenuItems"/> before pushing this state!
    /// </summary>
    public static DynamicMenuState I { get; private set; }

    //Input
    public List<string> MenuItems { get; set; }

    //Output
    public int? SelectedItem { get; private set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        foreach (Transform child in dynamicMenuUI.transform)
        {
            Destroy(child.gameObject);
        }

        var textSlots = new List<TextSlot>();
        foreach (var menuItem in MenuItems)
        {
            var slot = Instantiate(itemTextPrefab, dynamicMenuUI.transform);
            slot.SetText(menuItem);
            textSlots.Add(slot);
        }

        dynamicMenuUI.SetItems(textSlots);
        dynamicMenuUI.gameObject.SetActive(true);
        dynamicMenuUI.OnSelected += OnItemSelected;
        dynamicMenuUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        dynamicMenuUI.HandleUpdate();
    }

    public override void Exit()
    {
        dynamicMenuUI.ClearItems();

        dynamicMenuUI.gameObject.SetActive(false);
        dynamicMenuUI.OnSelected -= OnItemSelected;
        dynamicMenuUI.OnBack -= OnBack;
    }

    private void OnItemSelected(int selection)
    {
        SelectedItem = selection;
        gc.stateMachine.Pop();
    }

    private void OnBack()
    {
        SelectedItem = null;
        gc.stateMachine.Pop();
    }
}
