using System;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : SelectionUI<TextSlot>
{
    void Start()
    {
        GameController.I.PauseGame(true);
        var textSlots = GetComponentsInChildren<TextSlot>().ToList();
        if (SavingSystem.i.CheckIfSaveExists("saveSlot1"))
        {
            SetItems(textSlots);
        }
        else
        {
            SetItems(textSlots.TakeLast(3).ToList());
            textSlots.First().GetComponent<TMP_Text>().color = Color.gray;
        }

        OnSelected += OnItemSelected;
        OnBack += OnMenuBack;
    }

    private void OnItemSelected(int obj)
    {
        if (obj == 0 && !SavingSystem.i.CheckIfSaveExists("saveSlot1"))
        {
            obj++;
        }

        switch (obj)
        {
            case 0:
                // Continue
                DontDestroyOnLoad(gameObject);

                SceneManager.LoadScene(1);
                SavingSystem.i.Load("saveSlot1");
                GameController.I.PauseGame(false);

                Destroy(gameObject);
                break;
            case 1:
                // New Game
                SavingSystem.i.Delete("saveSlot1");
                SceneManager.LoadScene(1);
                GameController.I.PauseGame(false);
                break;
            case 2:
                // Settings
                break;
            case 3:
                // Exit
                Application.Quit();
                break;
        }
    }

    private void OnMenuBack()
    {
        // Exit
        Application.Quit();
    }

    void Update()
    {
        HandleUpdate();
    }
}
