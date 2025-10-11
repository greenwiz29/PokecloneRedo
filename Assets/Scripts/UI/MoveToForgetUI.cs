using System;
using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;

public class MoveToForgetUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TMP_Text> moveTexts;
    public event Action<int> OnSelectionChanged;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }
        moveTexts[currentMoves.Count].text = newMove.Name;

        SetItems(moveTexts.Select(s => s.GetComponent<TextSlot>()).ToList());
    }
}
