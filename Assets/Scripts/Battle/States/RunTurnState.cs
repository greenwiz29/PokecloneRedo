using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="BattleActions"/> before pushing this state!
/// </summary>
public class RunTurnState : State<BattleSystem>
{
    [SerializeField] MoveBase struggle;

    /// <summary>
    /// Make sure to set <see cref="BattleActions"/> before pushing this state!
    /// </summary>
    public static RunTurnState I { get; private set; }

    // Inputs
    public List<BattleAction> BattleActions { get; set; }

    void Awake()
    {
        I = this;
    }

    BattleDialogBox dialogBox;


    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        dialogBox = bs.DialogBox;

        StartCoroutine(RunTurns());
    }


    IEnumerator RunTurns()
    {
        foreach (var action in BattleActions)
        {
            if (action.IsInvalid)
                continue;

            switch (action.Type)
            {
                case BattleActionType.Move:
                    action.User.Pokemon.CurrentMove = action.SelectedMove;
                    yield return RunMove(action.User, action.Target, action.SelectedMove);
                    yield return RunAfterTurn(action.User);
                    break;

                case BattleActionType.Switch:
                    yield return bs.SwitchPokemon(action.SelectedPokemon, action.User);
                    break;

                case BattleActionType.Item:
                    if (action.SelectedItem is PokeballItem)
                    {
                        yield return bs.ThrowPokeball(action.SelectedItem as PokeballItem);
                    }
                    break;

                case BattleActionType.Run:
                    yield return TryToRun();
                    break;
            }
            if (bs.IsBattleOver)
            {
                yield break;
            }
        }
        bs.ClearBattleActions();

        if (!bs.IsBattleOver)
        {
            bs.StateMachine.ChangeState(ActionSelectionState.I);
        }
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
                yield return HandlePokemonFainted(target, source, damageDetails);
            }
            else
            {
                // Apply exp gain for attacker
                yield return bs.ApplyExpGain(source, target, false, damageDetails, false);
                // Apply exp gain for defender
                yield return bs.ApplyExpGain(target, source, false, damageDetails, false);
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
            yield return HandlePokemonFainted(source, null);
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

    IEnumerator AfterFainting(BattleUnit faintedUnit)
    {
        //Remove the fainted pokemon's action
        var actionToRemove = BattleActions.FirstOrDefault(a => a.User == faintedUnit);
        if (actionToRemove != null)
        {
            actionToRemove.IsInvalid = true;
        }
        
        if (faintedUnit.IsPlayerUnit)
        {
            var activePokemon = bs.PlayerUnits.Select(u => u.Pokemon).Where(p => p.HP > 0).ToList();
            var next = bs.PlayerParty.GetHealthyPokemon(activePokemon);
            if (next == null)
            {
                if (activePokemon.Count == 0)
                    bs.BattleOver(false);
                else
                {
                    // No new pokmeon to send out, but we still have one active unit, so the battle can continue. Clear the hud for the fainted unit.
                    faintedUnit.Clear();
                    bs.PlayerUnits.Remove(faintedUnit);

                    // Attacks targeted at the fainted unit should be changed
                    var actionsToChange = BattleActions.Where(a => a.Target == faintedUnit).ToList();
                    actionsToChange.ForEach(a => a.Target = bs.PlayerUnits.First());
                }
            }
            else
            {
                yield return GameController.I.stateMachine.PushAndWait(PartyState.I);
                yield return bs.SwitchPokemon(PartyState.I.SelectedPokemon, faintedUnit);
            }
        }
        else
        {
            if (!bs.IsTrainerBattle)
            {
                bs.BattleOver(true);
                yield break;
            }
            var activePokemon = bs.EnemyUnits.Select(u => u.Pokemon).Where(p => p.HP > 0).ToList();
            var next = bs.TrainerParty.GetHealthyPokemon(activePokemon);
            if (next == null)
            {
                if (activePokemon.Count == 0)
                    bs.BattleOver(true);
                else
                {
                    // No new pokmeon to send out, but we still have one active unit, so the battle can continue. Clear the hud for the fainted unit.
                    faintedUnit.Clear();
                    bs.EnemyUnits.Remove(faintedUnit);

                    // Attacks targeted at the fainted unit should be changed
                    var actionsToChange = BattleActions.Where(a => a.Target == faintedUnit).ToList();
                    actionsToChange.ForEach(a => a.Target = bs.EnemyUnits.First());
                }
            }
            else
            {
                if (bs.UnitCount == 1)
                {
                    AboutToUseState.I.NewPokemon = next;
                    AboutToUseState.I.UnitToSwitch = faintedUnit;
                    yield return bs.StateMachine.PushAndWait(AboutToUseState.I);
                }
                else
                {
                    yield return bs.SendNextTrainerPokemon(faintedUnit);
                }
            }
        }
    }

    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, BattleUnit attackerUnit = null, DamageDetails damageDetails = null)
    {
        string enemy;
        // target unit fainted
        faintedUnit.PlayFaintAnimation();
        enemy = !faintedUnit.IsPlayerUnit ? "Enemy " : "";
        yield return dialogBox.TypeDialog($"{enemy}{faintedUnit.Pokemon.Name} fainted.");
        yield return new WaitForSeconds(1f);

        if (!faintedUnit.IsPlayerUnit)
        {
            for (int i = 0; i < bs.ActivePlayerUnitsCount; i++)
            {
                BattleUnit unit = bs.PlayerUnits[i];
                yield return bs.ApplyExpGain(unit, faintedUnit, true, damageDetails);
            }
        }
        else
        {
            if (attackerUnit != null)
                yield return bs.ApplyExpGain(attackerUnit, faintedUnit, true, damageDetails, false);
        }

        yield return AfterFainting(faintedUnit);
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

        var playerSpeed = bs.PlayerUnits[0].Pokemon.Speed;
        var enemySpeed = bs.EnemyUnits[0].Pokemon.Speed;

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
