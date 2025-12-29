using System.Collections;

public abstract class BattleEvent
{
    public abstract IEnumerator Execute(BattleSystem bs);
}
