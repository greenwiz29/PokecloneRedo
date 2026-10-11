using System;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; }

    public int PP { get; set; }
    public int MaxPP => Base.PP;

    public Move(MoveBase mBase)
    {
        if (mBase == null)
            throw new ArgumentNullException(nameof(mBase));

        Base = mBase;
        PP = Base.PP;
    }

    public Move(MoveSaveData saveData)
    {
        if (saveData == null)
            throw new ArgumentNullException(nameof(saveData));

        MoveBase moveBase = null;

        // New saves resolve by stable ID first.
        if (!string.IsNullOrWhiteSpace(saveData.id))
        {
            MoveDB.TryGetObjectById(saveData.id, out moveBase);
        }

        // Backward compatibility for old saves, or saves whose ID
        // cannot be resolved.
        if (moveBase == null && !string.IsNullOrWhiteSpace(saveData.name))
        {
            moveBase = MoveDB.GetObjectByName(saveData.name);

            if (moveBase != null && !string.IsNullOrWhiteSpace(saveData.id))
            {
                Debug.LogWarning(
                    $"Could not resolve move ID '{saveData.id}'. " +
                    $"Loaded legacy asset name '{saveData.name}' instead.",
                    moveBase);
            }
        }

        if (moveBase == null)
        {
            throw new InvalidOperationException(
                $"Could not load saved move. ID: '{saveData.id}', " +
                $"legacy name: '{saveData.name}'.");
        }

        Base = moveBase;
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        return new MoveSaveData
        {
            id = Base.Id,
            name = Base.name,
            pp = PP
        };
    }

    public void IncreasePP(int amount)
    {
        PP = Mathf.Clamp(PP + amount, 0, MaxPP);
    }
}

[Serializable]
public class MoveSaveData
{
    // Stable identifier used by new saves.
    public string id;

    // Retained for backward compatibility with existing saves.
    public string name;

    public int pp;
}
