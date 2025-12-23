using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

// The authoritative input structure
public struct NetworkInputData : INetworkInput
{
    public Vector2 direction;
}

public class GlobalManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject playerPrefab;

    async void StartGame(GameMode mode)
    {
        GameObject runnerObj = new GameObject("Runner_" + mode.ToString());
        NetworkRunner runner = runnerObj.AddComponent<NetworkRunner>();

        runnerObj.AddComponent<NetworkSceneManagerDefault>();
        runnerObj.AddComponent<RunnerEnableVisibility>();

        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (activeSceneIndex < 0) activeSceneIndex = 0; 

        var sceneInfo = new NetworkSceneInfo();
        var sceneRef = SceneRef.FromIndex(activeSceneIndex);
        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "MultiPeerTestRoom",
            Scene = sceneInfo,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"Failed to Start {mode}: {result.ShutdownReason}");
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "Start Host"))
        {
            StartGame(GameMode.Host);
        }
        if (GUI.Button(new Rect(10, 60, 200, 40), "Start Client"))
        {
            StartGame(GameMode.Client);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3(player.RawEncoded * 3, 1, 0);
            runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        }

        // Fixed Fusion 2.0.2 logic: Compare PlayerId instead of the PlayerRef struct
        if (Camera.main != null)
        {
            var listener = Camera.main.GetComponent<AudioListener>();
            if (listener != null)
            {
                // Only the first local runner (Host) keeps the AudioListener enabled
                listener.enabled = (runner.LocalPlayer.PlayerId == 0);
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.direction.x = Input.GetAxisRaw("Horizontal");
        data.direction.y = Input.GetAxisRaw("Vertical");
        input.Set(data);
    }

    // Standard Fusion 2.0.2 Signatures
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}