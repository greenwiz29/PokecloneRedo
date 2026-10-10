using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using UnityEngine;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> moveTexts;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] Color comboHighlightColor = Color.yellow;
    Color _normalColor;

    void Start()
    {
        SetSelectionSettings(SelectionMode.GRID, 2);
        _normalColor = moveTexts[0].GetColor();
    }

    List<Move> _moves;
    
    public void SetMoves(List<Move> moves, HashSet<int> comboIndices = null)
    {
        _moves = moves;
        selection = Mathf.Min(selection, moves.Count - 1);

        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                bool isCombo = comboIndices != null && comboIndices.Contains(i);

                moveTexts[i].SetText(moves[i].Base.Name + (isCombo ? " *" : ""));
                moveTexts[i].SetColor(isCombo ? comboHighlightColor : _normalColor);
            }
            else
            {
                moveTexts[i].SetText("-");
                moveTexts[i].SetColor(_normalColor);
            }
        }
        SetItems(moveTexts.Take(moves.Count).ToList());
    }

    public override void UpdateSelectionUI()
    {
        base.UpdateSelectionUI();

        dialogBox.UpdateMoveDetails(_moves[selection]);
    }
}
