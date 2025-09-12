using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] DestinationID destinationPortal;
    [SerializeField] Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint;

	public bool TriggerRepeatedly => false;

	PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(Teleport());
    }

    Fader fader;

    void Start()
    {
        fader = FindAnyObjectByType<Fader>();
    }
    IEnumerator Teleport()
    {
        GameController.I.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsByType<LocationPortal>(FindObjectsSortMode.None).First(x => x != this && x.destinationPortal == destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.I.PauseGame(false);
    }

    public enum DestinationID { A, B, C, D, E }
}
