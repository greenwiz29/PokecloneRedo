using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;
    [SerializeField] float encounterRate = 6.25f;

    public string Name { get => name; }
    public Sprite Sprite { get => sprite; }
    public float EncounterRate => encounterRate;
    public Character Character => character;
    public PokemonParty Party { get; private set; }

    private Vector2 input;

    private Character character;

    void Awake()
    {
        character = GetComponent<Character>();
        Party = GetComponent<PokemonParty>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving && GameController.I.State == GameState.FreeRoam)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Interact());
        }
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GlobalSettings.I.InteractablesLayer | GlobalSettings.I.WaterLayer);
        if (collider != null)
        {
            var interactor = collider.GetComponent<IInteractable>();
            yield return interactor?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 0.15f, GlobalSettings.I.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IPlayerTriggerable>(out triggerable))
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                character.Animator.IsMoving = false;
                triggerable?.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;

                StoryItem storyItem = collider.gameObject.GetComponent<StoryItem>();
                if (storyItem != null && storyItem.blocksMovement)
                {
                    StartCoroutine(character.Move(character.PreviousTile, OnMoveOver));
                }
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
        {
            currentlyInTrigger = null;
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            partyData = GetComponent<PokemonParty>().Party.Select(p => p.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        Party.Party = saveData.partyData.Select(pd => new Pokemon(pd)).ToList();
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> partyData;
}
