using System;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : SelectionUI<TextSlot>
{
    [SerializeField] Image pokemonImage;
    void Start()
    {
        GameController.I.PauseGame(true);
        pokemonImage.sprite = GetRandomPokemonSprite();
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

    private Sprite GetRandomPokemonSprite()
    {
        return PokemonDB.GetRandomObject().FrontSprite;
    }

    private void OnItemSelected(int obj)
    {
        if (!SavingSystem.i.CheckIfSaveExists("saveSlot1"))
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
#if UNITY_EDITOR
                // This code will only run in the Unity Editor
                EditorApplication.ExitPlaymode();
#else
                // This code will run in a built game
                Application.Quit();
#endif
                break;
        }
    }

    private void OnMenuBack()
    {
        // Exit
#if UNITY_EDITOR
        // This code will only run in the Unity Editor
        EditorApplication.ExitPlaymode();
#else
        // This code will run in a built game
        Application.Quit();
#endif
    }

    void Update()
    {
        HandleUpdate();
    }
}
