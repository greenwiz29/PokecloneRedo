using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] List<TMP_Text> menuItems;

    int currentSelection = 0;
    public event Action<int> OnMenuSelected;
    public event Action OnBack;

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateMenuSelection(currentSelection);
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prev = currentSelection;
        MenuSelectionMethods.HandleListSelection(ref currentSelection, menuItems.Count - 1);

        if (currentSelection != prev)
            UpdateMenuSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnMenuSelected?.Invoke(currentSelection);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBack?.Invoke();
            CloseMenu();
        }
    }

    public void UpdateMenuSelection(int selection)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selection)
            {
                menuItems[i].color = GlobalSettings.I.HighlightedColor;
            }
            else
            {
                menuItems[i].color = GlobalSettings.I.DefaultFontColor;
            }
        }
    }

}
