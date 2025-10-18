using System.Collections;
using System.Linq;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="Moves"/> before pushing this state!
/// </summary>
public class RunTurnState : State<BattleSystem>
{
    [SerializeField] MoveBase struggle;

    /// <summary>
    /// Make sure to set <see cref="Moves"/> before pushing this state!
    /// </summary>
    public static RunTurnState I { get; private set; }

    void Awake()
    {
        I = this;
    }

    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogBox dialogBox;


    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        playerUnit = bs.PlayerUnit;
        enemyUnit = bs.EnemyUnit;
        dialogBox = bs.DialogBox;

        StartCoroutine(RunTurns(bs.SelectedAction));
    }


    IEnumerator RunTurns(BattleAction playerAction)
    {
        switch (playerAction)
        {
            case BattleAction.Move:
                playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[bs.SelectedMove];
                enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();
                bool playerGoesFirst = CheckTurnOrder();

                var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
                var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;
                var secondPokemon = secondUnit.Pokemon;

                yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(firstUnit);
                if (bs.IsBattleOver)
                    yield break;

                if (secondPokemon.HP > 0)
                {
                    yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                    yield return RunAfterTurn(secondUnit);
                    if (bs.IsBattleOver)
                        yield break;
                }
                break;

            case BattleAction.Switch:
                yield return bs.SwitchPokemon(bs.SelectedPokemon);

                yield return RunEnemyTurn();
                break;

            case BattleAction.Item:
                if (bs.SelectedItem is PokeballItem)
                {
                    yield return bs.ThrowPokeball(bs.SelectedItem as PokeballItem);
                    if (bs.IsBattleOver)
                    {
                        yield break;
                    }
                }
                yield return RunEnemyTurn();
                break;

            case BattleAction.Run:
                yield return TryToRun();

                yield return RunEnemyTurn();
                break;
        }

        if (!bs.IsBattleOver)
        {
            bs.StateMachine.ChangeState(ActionSelectionState.I);
        }
    }

    private IEnumerator RunEnemyTurn()
    {
        var enemyMove = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, enemyMove);
        yield return RunAfterTurn(enemyUnit);
        if (bs.IsBattleOver)
            yield break;
    }

    private bool CheckTurnOrder()
    {
        int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
        int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

        bool playerGoesFirst;

        if (enemyMovePriority > playerMovePriority)
            playerGoesFirst = false;
        else if (playerMovePriority > enemyMovePriority)
            playerGoesFirst = true;
        else
            playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        return playerGoesFirst;
    }

    private IEnumerator RunMove(BattleUnit source, BattleUnit target, Move move)
    {
        if (!source.Pokemon.OnBeforeMove())
        {
            yield return ShowStatusChanges(source.Pokemon);
            yield return source.HUD.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(source.Pokemon);

        var enemy = source.IsPlayerUnit ? "" : "Enemy ";

        if (move != null)
        {
            move.PP--;
        }
        else
        {
            move = new Move(struggle);
        }

        yield return dialogBox.TypeDialog($"{enemy}{source.Pokemon.Name} used {move.Base.Name}");

        if (!CheckIfMoveHits(move, source.Pokemon, target.Pokemon))
        {
            yield return dialogBox.TypeDialog("... but it missed.");
        }
        else
        {
            DamageDetails damageDetails = null;

            source.PlayAttackAnimation();
            yield return new WaitForSeconds(0.75f);

            target.PlayHitAnimation();

            if (move.Base.MoveType == MoveType.Status)
            {
                yield return RunMoveEffects(source, target, move.Base.Effects, move.Base.Target);
            }
            else
            {
                damageDetails = target.Pokemon.ApplyDamage(move, source.Pokemon);

                yield return target.HUD.WaitForHPUpdate();

                yield return ShowDamageDetails(damageDetails);
            }

            var secondaries = move.Base.SecondaryEffects;
            if (secondaries != null && secondaries.Count > 0 && target.Pokemon.HP <= 0)
            {
                foreach (var secEffect in secondaries)
                {
                    var rand = UnityEngine.Random.Range(1, 101);
                    if (rand <= secEffect.Chance)
                    {
                        yield return RunMoveEffects(source, target, secEffect, secEffect.Target);
                    }
                }
            }

            if (target.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(target, damageDetails);
            }
            else
            {
                yield return bs.ApplyExpGain(enemyUnit, false, damageDetails);
            }
        }
    }

    private IEnumerator RunMoveEffects(BattleUnit source, BattleUnit target, MoveEffects effects, MoveTarget moveTarget)
    {
        // Stat boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.Pokemon.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.Pokemon.ApplyBoosts(effects.Boosts);
            }
        }
        // Conditions
        if (effects.Status != ConditionID.none)
        {
            target.Pokemon.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.Pokemon.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source.Pokemon);
        yield return ShowStatusChanges(target.Pokemon);
    }

    IEnumerator RunAfterTurn(BattleUnit source)
    {
        if (bs.IsBattleOver)
            yield break;

        source.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(source.Pokemon);
        yield return source.HUD.WaitForHPUpdate();

        // Check if source pokemon fainted after status effect
        if (source.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(source);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else if (accuracy < 0)
        {
            moveAccuracy /= boostValues[-accuracy];
        }
        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else if (evasion < 0)
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var next = bs.PlayerParty.GetHealthyPokemon();
            if (next == null)
            {
                bs.BattleOver(false);
            }
            else
            {
                yield return GameController.I.stateMachine.PushAndWait(PartyState.I);
                yield return bs.SwitchPokemon(PartyState.I.SelectedPokemon);
            }
        }
        else
        {
            if (!bs.IsTrainerBattle)
                bs.BattleOver(true);
            else
            {
                var next = bs.TrainerParty.GetHealthyPokemon();
                if (next == null)
                {
                    bs.BattleOver(true);
                }
                else
                {
                    AboutToUseState.I.NewPokemon = next;
                    yield return bs.StateMachine.PushAndWait(AboutToUseState.I);
                }
            }
        }
    }

    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, DamageDetails damageDetails = null)
    {
        string enemy;
        // target unit fainted
        faintedUnit.PlayFaintAnimation();
        enemy = !faintedUnit.IsPlayerUnit ? "Enemy " : "";
        yield return dialogBox.TypeDialog($"{enemy}{faintedUnit.Pokemon.Name} fainted.");
        yield return new WaitForSeconds(1f);

        if (!faintedUnit.IsPlayerUnit)
        {
            yield return bs.ApplyExpGain(faintedUnit, true, damageDetails);
        }

        yield return CheckForBattleOver(faintedUnit);
    }

    IEnumerator ShowDamageDetails(DamageDetails details)
    {
        if (details.Crit > 1f)
        {
            yield return dialogBox.TypeDialog($"A critical hit!");
        }

        if (details.TypeEffectiveness > 2f)
        {
            yield return dialogBox.TypeDialog($"It's amazingly effective!");
        }
        else if (details.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"It's really effective!");
        }
        else if (details.TypeEffectiveness == 0.0f)
        {
            yield return dialogBox.TypeDialog("It has no effect.");
        }
        else if (details.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();

            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator TryToRun()
    {
        if (bs.IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            yield break;
        }

        bs.EscapeAttempts++;

        var playerSpeed = playerUnit.Pokemon.Speed;
        var enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            bs.BattleOver(true);
        }
        else
        {
            float f = playerSpeed * 128 / enemySpeed + 30 * bs.EscapeAttempts;
            f %= 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
            }
        }
    }

}
