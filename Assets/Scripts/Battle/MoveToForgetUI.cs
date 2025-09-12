using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoveToForgetUI : MonoBehaviour
{
    [SerializeField] List<TMP_Text> moveTexts;
    int currentSelection = 0;
    public event Action<int> OnSelectionChanged;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }
        moveTexts[currentMoves.Count].text = newMove.Name;

        UpdateMoveSelection(currentSelection);
    }

    public void HandleUpdate(Action<int> onSelected)
    {
        int prev = currentSelection;
        MenuSelectionMethods.HandleListSelection(ref currentSelection, Pokemon.maxMoves);

        if (currentSelection != prev)
        {
            UpdateMoveSelection(currentSelection);
            OnSelectionChanged?.Invoke(currentSelection);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < Pokemon.maxMoves + 1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = GlobalSettings.I.HighlightedColor;
            }
            else
            {
                moveTexts[i].color = GlobalSettings.I.DefaultFontColor;
            }
        }
    }
}
