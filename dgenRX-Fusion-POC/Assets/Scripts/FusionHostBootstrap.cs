using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class FusionHostBootstrap : MonoBehaviour
{
    [SerializeField] private string sessionName = "dgenrx-baseline";
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private NetworkRunner _runner;

    private async void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            Debug.LogError($"FusionHostBootstrap is on '{gameObject.name}' but there is NO NetworkRunner on it.");
            return;
        }

        _runner.ProvideInput = true;

        Debug.Log("FusionHostBootstrap: Starting Fusion Host...");

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            SceneManager = null
        });

        if (result.Ok)
        {
            Debug.Log("FusionHostBootstrap: Host started OK.");
            SpawnPlayer();
        }
        else
        {
            Debug.LogError($"FusionHostBootstrap: Host failed: {result.ShutdownReason}");
        }
    }

    private void SpawnPlayer()
    {
        if (!_runner.IsServer)
            return;

        _runner.Spawn(
            playerPrefab,
            new Vector3(0, 1, 0),
            Quaternion.identity
        );

        Debug.Log("FusionHostBootstrap: Player cube spawned.");
    }
}
