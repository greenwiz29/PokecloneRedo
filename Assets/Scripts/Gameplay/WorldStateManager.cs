using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central store for persistent world state: boolean flags and integer story-thread counters.
/// Attach to a persistent GameObject (e.g. your GameManager or Player).
///
/// Usage:
///   WorldStateManager.I.SetFlag(myFlagAsset);
///   WorldStateManager.I.IsFlagSet(myFlagAsset);
///   WorldStateManager.I.SetCounter("RivalArc", 2);
///   WorldStateManager.I.GetCounter("RivalArc");
/// </summary>
public class WorldStateManager : MonoBehaviour, ISavable
{
    public static WorldStateManager I { get; private set; }

    // Flags are stored by the WorldStateFlag asset's name.
    // Only set flags are stored — absence means false.
    readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

    // Counters are stored by an arbitrary string key you define per story thread.
    // 0 means "not started / no progress".
    readonly Dictionary<string, int> counters = new Dictionary<string, int>();

    public event Action OnFlagsUpdated;
    public event Action OnCountersUpdated;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    // -------------------------------------------------------------------------
    // Flags — asset overloads (preferred: drag a WorldStateFlag asset in Inspector)
    // -------------------------------------------------------------------------

    public void SetFlag(WorldStateFlag flag)
    {
        if (flag == null) return;
        flags[flag.FlagName] = true;
        OnFlagsUpdated?.Invoke();
    }

    public void ClearFlag(WorldStateFlag flag)
    {
        if (flag == null) return;
        flags.Remove(flag.FlagName);
        OnFlagsUpdated?.Invoke();
    }

    public bool IsFlagSet(WorldStateFlag flag)
    {
        if (flag == null) return false;
        return flags.TryGetValue(flag.FlagName, out var value) && value;
    }

    // -------------------------------------------------------------------------
    // Flags — raw string overloads (used by Yarn commands and dialog scripts)
    // -------------------------------------------------------------------------

    public void SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        flags[flagName] = true;
        OnFlagsUpdated?.Invoke();
    }

    public void ClearFlagByName(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        flags.Remove(flagName);
        OnFlagsUpdated?.Invoke();
    }

    public bool IsFlagSet(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;
        return flags.TryGetValue(flagName, out var value) && value;
    }

    // -------------------------------------------------------------------------
    // Counters (story thread progress)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets a story thread counter to an explicit value.
    /// Use this when you want exact control (e.g. restoring from save).
    /// </summary>
    public void SetCounter(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) return;
        counters[key] = value;
        OnCountersUpdated?.Invoke();
    }

    /// <summary>
    /// Advances a story thread counter by 1.
    /// Typical usage: WorldStateManager.I.AdvanceCounter("RivalArc");
    /// </summary>
    public void AdvanceCounter(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        counters[key] = GetCounter(key) + 1;
        OnCountersUpdated?.Invoke();
    }

    /// <summary>
    /// Returns the current value of a story thread counter, or 0 if never set.
    /// </summary>
    public int GetCounter(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0;
        return counters.TryGetValue(key, out var value) ? value : 0;
    }

    // -------------------------------------------------------------------------
    // ISavable
    // -------------------------------------------------------------------------

    public object CaptureState()
    {
        return new WorldStateSaveData
        {
            flags = new List<string>(flags.Keys),
            counters = new Dictionary<string, int>(counters)
        };
    }

    public void RestoreState(object state)
    {
        if (state is not WorldStateSaveData saveData) return;

        flags.Clear();
        if (saveData.flags != null)
            foreach (var name in saveData.flags)
                flags[name] = true;

        counters.Clear();
        if (saveData.counters != null)
            foreach (var kvp in saveData.counters)
                counters[kvp.Key] = kvp.Value;

        OnFlagsUpdated?.Invoke();
        OnCountersUpdated?.Invoke();
    }
}

[Serializable]
public class WorldStateSaveData
{
    // Only storing flag names — their presence means true.
    public List<string> flags;
    public Dictionary<string, int> counters;
}