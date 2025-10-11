using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using UnityEngine;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> moveTexts;
    [SerializeField] BattleDialogBox dialogBox;

    void Start()
    {
        SetSelectionSettings(SelectionMode.GRID, 2);
    }

    List<Move> _moves;
    public void SetMoves(List<Move> moves)
    {
        _moves = moves;
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].SetText(moves[i].Base.Name);
            else
                moveTexts[i].SetText("-");
        }
        SetItems(moveTexts.Take(moves.Count).ToList());
    }

    public override void UpdateSelectionUI()
    {
        base.UpdateSelectionUI();

        dialogBox.UpdateMoveDetails(_moves[selection]);
    }    
}
