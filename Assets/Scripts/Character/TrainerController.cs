using System.Collections;
using UnityEngine;

public class TrainerController : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;
    [SerializeField] GameObject exclamation, fov;
    [SerializeField] Dialog preBattleDialog, postBattleDialog;
    [SerializeField] int battleUnitCount = 1;

    // state
    bool battleLost = false;

    Character character;

    public string Name { get => name; }
    public Sprite Sprite { get => sprite; }
    public int BattleUnitCount => battleUnitCount;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            yield return DialogManager.I.ShowDialog(preBattleDialog);

            GameController.I.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogManager.I.ShowDialog(postBattleDialog);
        }
    }

    void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        GameController.I.stateMachine.Push(CutsceneState.I);
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        diff -= diff.normalized;

        var moveVec = new Vector2(Mathf.Round(diff.x), Mathf.Round(diff.y));

        yield return character.Move(moveVec);

        yield return DialogManager.I.ShowDialog(preBattleDialog);

        GameController.I.stateMachine.Pop();

        GameController.I.StartTrainerBattle(this);
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        float offsetX = 0f;
        float offsetY = 0f;
        switch (dir)
        {
            case FacingDirection.Right:
                angle = 180f;
                offsetY = -0.5f;
                break;
            case FacingDirection.Up:
                angle = 270f;
                offsetX = 0.3f;
                break;
            case FacingDirection.Down:
                angle = 90f;
                offsetX = -0.3f;
                break;
            default:
                break;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
        fov.transform.localPosition += new Vector3(offsetX, offsetY);
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.SetActive(false);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        if (battleLost)
        {
            fov.SetActive(false);
        }
    }
}
