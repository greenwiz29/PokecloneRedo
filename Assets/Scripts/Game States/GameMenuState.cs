using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class GameMenuState : State<GameController>
{
    [SerializeField] MenuController menuController;

    public static GameMenuState I { get; private set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        menuController.gameObject.SetActive(true);
        menuController.OnSelected += OnSelected;
        menuController.OnBack += OnBack;
    }

    private void OnBack()
    {
        gc.stateMachine.Pop();
    }

    private void OnSelected(int selectedItem)
    {
        menuController.SetSelectionSettings(GDEUtils.UI.SelectionMode.LIST);
        switch (selectedItem)
        {
            case 0: // Storage
                gc.stateMachine.Push(StorageState.I);
                break;
            case 1: // Pokemon
                if (gc.Player.Party.Pokemon.Count > 0)
                    gc.stateMachine.Push(PartyState.I);
                break;
            case 2: // Bag
                gc.stateMachine.Push(InventoryState.I);
                break;
            case 3: // Save
                StartCoroutine(SaveSelected());
                break;
            case 4: // Load
                StartCoroutine(LoadSelected());
                break;
        }
    }

    IEnumerator SaveSelected()
    {
        yield return Fader.I.FadeIn(0.5f);
        // TODO: select save slot, and confirm if overwriting
        SavingSystem.i.Save("saveSlot1");
        yield return Fader.I.FadeOut(0.5f);
        OnBack();
    }

    IEnumerator LoadSelected()
    {
        yield return Fader.I.FadeIn(0.5f);
        // TODO: select save slot
        SavingSystem.i.Load("saveSlot1");
        yield return Fader.I.FadeOut(0.5f);
        OnBack();
    }

    public override void Execute()
    {
        menuController.HandleUpdate();
    }

    public override void Exit()
    {
        menuController.gameObject.SetActive(false);
        menuController.OnSelected -= OnSelected;
        menuController.OnBack -= OnBack;
    }
}
