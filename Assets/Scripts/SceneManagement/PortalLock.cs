using UnityEngine;

public abstract class PortalLock : ScriptableObject
{
    public abstract bool IsUnlocked();
    public virtual string LockedMessage => "It's locked.";
}
