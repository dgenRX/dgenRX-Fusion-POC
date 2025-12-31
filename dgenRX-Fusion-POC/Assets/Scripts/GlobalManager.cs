using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global manager for network setup. Creates NetworkRunner and handles player spawning.
/// This is a MonoBehaviour (not NetworkBehaviour) because it creates the runner itself.
/// </summary>
public class GlobalManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject PlayerPrefab;
    public GameObject NetworkLogicPrefab;
    public GameObject PokerGameManagerPrefab;
    
    private NetworkRunner _localRunner;
    private Rect _windowRect = new Rect(20, 20, 300, 150);

    // Called by a UI Button in the scene (DealButton - may be obsolete now that game auto-starts)
    public void OnDealButtonClicked()
    {
        // Game now auto-starts when 2 players join, so this button may not be needed
        // Keeping for backwards compatibility with scene setup
    }

    private void OnGUI()
    {
        if (NetworkRunner.Instances.Count < 2)
        {
            _windowRect = GUILayout.Window(0, _windowRect, DrawConnectionWindow, "Network Setup");
        }
    }

    private void DrawConnectionWindow(int windowID)
    {
        if (GUILayout.Button("Start Host", GUILayout.Height(40))) StartGame(GameMode.Host);
        if (GUILayout.Button("Start Client", GUILayout.Height(40))) StartGame(GameMode.Client);
    }

    async void StartGame(GameMode mode)
    {
        GameObject runnerObj = new GameObject("Runner_" + mode);
        _localRunner = runnerObj.AddComponent<NetworkRunner>();
        _localRunner.ProvideInput = true;
        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();
        _localRunner.AddCallbacks(this);

        var result = await _localRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "PokerTestRoom",
            CustomLobbyName = "DegenRxLobby",
            Scene = SceneRef.FromIndex(0),
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"[Global] Failed to start {mode}: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Only the Host spawns the singleton NetworkLogic (Deck) and PokerGameManager
            if (player == runner.LocalPlayer)
            {
                if (NetworkLogicPrefab != null)
                {
                    runner.Spawn(NetworkLogicPrefab, Vector3.zero, Quaternion.identity);
                }
                
                // Spawn the PokerGameManager (only once, by the Host)
                if (PokerGameManagerPrefab != null)
                {
                    runner.Spawn(PokerGameManagerPrefab, Vector3.zero, Quaternion.identity);
                }
            }

            // Spawn players at the edge of the plane (further from center)
            Vector3 spawnPos = new Vector3(player.RawEncoded % 2 == 0 ? -5 : 5, 1, 0);
            runner.Spawn(PlayerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Input handled by PlayerCubeMovement component on player prefab
        // No need to duplicate input handling here
    }

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
