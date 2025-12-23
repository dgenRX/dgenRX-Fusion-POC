using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
}

public class GlobalManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef playerPrefab; 
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        if (_runner != null) _runner.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        if (_runner != null) _runner.RemoveCallbacks(this);
    }

    async void StartGame(GameMode mode)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
        }

        // Fusion 2.0 standard component for basic scene handling
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        _runner.ProvideInput = true;

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "", 
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager
        });
    }

    private void OnGUI()
    {
        if (_runner == null || !_runner.IsRunning)
        {
            if (GUI.Button(new Rect(10, 10, 200, 40), "Host")) StartGame(GameMode.Host);
            if (GUI.Button(new Rect(10, 60, 200, 40), "Join")) StartGame(GameMode.Client);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"[SPAWN] Spawning Cube for Player {player}");
            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        runner.ProvideInput = true;
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

        input.Set(data);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            if (networkObject != null) runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    // MANDATORY FUSION 2.0 CALLBACK SIGNATURES
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { } 
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}