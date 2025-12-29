using System.Collections;

public class DialogBattleEvent : BattleEvent
{
    string message;

    public DialogBattleEvent(string message)
    {
        this.message = message;
    }

    public override IEnumerator Execute(BattleSystem bs)
    {
        yield return bs.DialogBox.TypeDialog(message);
    }
}
