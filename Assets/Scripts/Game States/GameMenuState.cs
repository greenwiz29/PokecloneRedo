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
		switch (selectedItem)
        {
            case 0: // Pokemon
                gc.stateMachine.Push(GamePartyState.I);
                break;
            case 1: // Bag
                // inventoryUI.gameObject.SetActive(true);
                // state = GameState.Bag;
                break;
            case 2: // Save
                SavingSystem.i.Save("saveSlot1");
                OnBack();
                break;
            case 3: // Load
                SavingSystem.i.Load("saveSlot1");
                OnBack();
                break;
        }
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
