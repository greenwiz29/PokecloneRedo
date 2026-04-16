using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationID destinationPortal;
    [SerializeField] Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint;

	public bool TriggerRepeatedly => false;

	PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    Fader fader;

    void Start()
    {
        fader = FindAnyObjectByType<Fader>();
    }
    
    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.I.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        // Switch scene
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsByType<Portal>().First(x => x != this && x.destinationPortal == destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.I.PauseGame(false);

        Destroy(gameObject);
    }

    public enum DestinationID { A, B, C, D, E }
}
