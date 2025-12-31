using UnityEngine;

[CreateAssetMenu(menuName = "Portals/Locks/Quest Lock")]
public class QuestPortalLock : PortalLock
{
    [SerializeField] Quest quest;

    public override bool IsUnlocked()
        => quest.Status == QuestStatus.Completed;

    public override string LockedMessage
        => "The door is locked for now.";
}
