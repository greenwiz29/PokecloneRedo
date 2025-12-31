using UnityEngine;

[CreateAssetMenu(menuName = "Portals/Locks/Gym Badge Lock")]
public class GymBadgePortalLock : PortalLock
{
    [SerializeField] GymBadge badge;

    public override bool IsUnlocked()
        => Inventory.GetPlayerInventory().HasItem(badge);
}
