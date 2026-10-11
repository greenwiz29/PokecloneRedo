using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB : ScriptableObjectsDB<MoveBase>
{
    private static Dictionary<string, MoveBase> objectsById;

    public new static void Init()
    {
        ScriptableObjectsDB<MoveBase>.Init();
        RebuildIdIndex();
    }

    private static void EnsureIdIndex()
    {
        if (objects == null)
        {
            Init();
            return;
        }

        if (objectsById == null)
            RebuildIdIndex();
    }

    private static void RebuildIdIndex()
    {
        objectsById = new Dictionary<string, MoveBase>(
            StringComparer.Ordinal);

        var duplicateIds = new HashSet<string>(
            StringComparer.Ordinal);

        foreach (MoveBase move in objects.Values)
        {
            string id = move.Id?.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"Move asset '{move.name}' has no stable ID.",
                    move);
                continue;
            }

            if (objectsById.ContainsKey(id))
            {
                Debug.LogError(
                    $"Duplicate move ID '{id}'. " +
                    $"Asset '{move.name}' conflicts with another move.",
                    move);

                duplicateIds.Add(id);
                continue;
            }

            objectsById.Add(id, move);
        }

        // Don't allow an ambiguous ID to resolve to an arbitrary asset.
        foreach (string duplicateId in duplicateIds)
            objectsById.Remove(duplicateId);
    }

    public static bool TryGetObjectById(string id, out MoveBase move)
    {
        EnsureIdIndex();

        if (!string.IsNullOrWhiteSpace(id))
            return objectsById.TryGetValue(id.Trim(), out move);

        move = null;
        return false;
    }

    public static MoveBase GetObjectById(string id)
    {
        if (TryGetObjectById(id, out MoveBase move))
            return move;

        Debug.LogError($"Move with ID '{id}' was not found in the database.");
        return null;
    }
}