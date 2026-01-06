using System.Collections;
using System.Linq;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] string returnEntryPointId = "Return";

    [Header("Portal Rules")]
    [SerializeField] PortalDirection direction = PortalDirection.TwoWay;
    [SerializeField] PortalLock lockCondition;

    [Header("Destination")]
    [SerializeField] string targetSceneName;
    [SerializeField] string entryPointId;
    public bool TriggerRepeatedly => false;
    public string TargetSceneName => targetSceneName;
    public string EntryPointId => entryPointId;
    public string ReturnEntryPointId => returnEntryPointId;
    public PortalDirection Direction => direction;
    public PortalLock LockCondition => lockCondition;
    public SceneDetails TargetScene =>
        FindObjectsByType<SceneDetails>(FindObjectsSortMode.None)
            .FirstOrDefault(s => s.SceneName == targetSceneName);
#if UNITY_EDITOR
    public void SetEditorValues(string sceneName, string entryId)
    {
        targetSceneName = sceneName;
        entryPointId = entryId;
    }
#endif

    Fader fader;
    SceneEntryPoint entry;
    Vector2 entryPoint;

    void Awake()
    {
        fader = FindAnyObjectByType<Fader>();

    }

    public void OnPlayerTriggered(PlayerController player)
    {
        StartCoroutine(Teleport(player));
    }

    IEnumerator Teleport(PlayerController player)
    {
        if (lockCondition != null && !lockCondition.IsUnlocked())
        {
            yield return DialogManager.I.ShowDialogText(lockCondition.LockedMessage);
            yield break;
        }

        GameController.I.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var scene = TargetScene;
        if (scene == null)
        {
            Debug.LogError($"Target scene '{targetSceneName}' not found or not loaded.");
            yield break;
        }

        GameController.I.TransitionToScene(scene);

        entry = scene.GetEntryPoint(entryPointId);
        if (entry == null)
        {
            Debug.LogError($"Entry point '{entryPointId}' not found in scene '{targetSceneName}'.");
            yield break;
        }

        // Move player
        entryPoint = entry.gameObject.transform.position;
        player.Character.SetPositionAndSnapToTile(entryPoint);

        yield return fader.FadeOut(0.5f);
        GameController.I.PauseGame(false);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (entry == null || TargetScene == null)
            return;

        Gizmos.color = direction == PortalDirection.OneWay
            ? Color.red
            : Color.green;

        Gizmos.DrawLine(transform.position, entryPoint);

        // Arrow head
        Vector3 dir = ((Vector3)entryPoint - transform.position).normalized;
        Vector3 right = Quaternion.Euler(0, 0, 25) * -dir;
        Vector3 left = Quaternion.Euler(0, 0, -25) * -dir;

        Gizmos.DrawLine(entryPoint, (Vector3)entryPoint + right * 0.5f);
        Gizmos.DrawLine(entryPoint, (Vector3)entryPoint + left * 0.5f);

        Handles.Label(
            (transform.position + (Vector3)entryPoint) / 2,
            TargetScene.SceneName
        );
    }
#endif

}

public enum PortalDirection { TwoWay, OneWay }