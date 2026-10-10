using UnityEngine;

/// <summary>
/// A named ScriptableObject asset representing a world state flag.
/// Drag this into Inspector fields instead of typing raw strings.
/// The actual set/cleared state lives in WorldStateManager at runtime.
///
/// Create via: Assets > Create > World > State Flag
/// </summary>
[CreateAssetMenu(menuName = "World/State Flag")]
public class WorldStateFlag : ScriptableObject
{
    [SerializeField] string flagName;

    /// <summary>
    /// The canonical string key used internally by WorldStateManager.
    /// Falls back to the asset name if flagName is left blank.
    /// </summary>
    public string FlagName => string.IsNullOrEmpty(flagName) ? name : flagName;
}