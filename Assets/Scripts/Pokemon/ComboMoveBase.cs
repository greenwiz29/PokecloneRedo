using UnityEngine;

[CreateAssetMenu(fileName = "ComboMove", menuName = "Pokemon/Create new Combo Move")]
public class ComboMoveBase : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] string description;

    // The two contributing moves (order doesn't matter)
    [SerializeField] MoveBase triggerMoveA;
    [SerializeField] MoveBase triggerMoveB;

    // The resulting move that actually fires
    [SerializeField] MoveBase resultMove;

    // Optional: a special animation/VFX different from resultMove's normal one
    [SerializeField] string comboAnimationId;

    [SerializeField] float comboMultiplier = 1.5f;
    public float ComboMultiplier => comboMultiplier;

    public string Name => name;
    public MoveBase TriggerMoveA => triggerMoveA;
    public MoveBase TriggerMoveB => triggerMoveB;
    public MoveBase ResultMove => resultMove;
    public string ComboAnimationId => comboAnimationId;

    public bool IsTriggeredBy(MoveBase a, MoveBase b)
    {
        return (triggerMoveA == a && triggerMoveB == b)
            || (triggerMoveA == b && triggerMoveB == a);
    }
}