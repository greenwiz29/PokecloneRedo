using System.Collections;

public class EvolutionRequestEvent : BattleEvent
{
    EvolutionRequest request;

    public EvolutionRequestEvent(EvolutionRequest request)
    {
        this.request = request;
    }

    public override IEnumerator Execute(BattleSystem bs)
    {
        if (request.Reason != null)
        {
            yield return bs.DialogBox.TypeDialog(request.Reason);
        }

        // 🔑 THIS is the important line:
        yield return EvolutionState.I.Evolve(request.Unit.Pokemon, request.ForcedEvolution, null);
    }
}
