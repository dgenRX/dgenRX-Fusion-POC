using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public GameObject PlayerPrefab;
    public GameObject NetworkLogicPrefab;
    public GameObject PokerGameManagerPrefab; // New: Prefab for the poker game manager
    
    private NetworkRunner _localRunner;
    private Rect _windowRect = new Rect(20, 20, 300, 150);

    [Networked] public bool DealRequested { get; set; }

    // Called by a UI Button in the scene
    public void OnDealButtonClicked()
    {
        if (_localRunner == null) return;
        RPC_RequestDeal();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDeal()
    {
        // This is executed on the Host when any client (or the host) calls it
        Debug.Log("[Global] Deal Request Received by Host via RPC.");
        DealRequested = true;
    }

    private void Update()
    {
        // Local input check for the Host only as a shortcut
        if (Object != null && Object.HasStateAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DealRequested = true;
            }
        }
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
        Debug.Log($"[Global] Starting {mode}...");
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

        if (result.Ok)
        {
            Debug.Log($"[Global] {mode} started successfully.");
        }
        else
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
                    Debug.Log("[GlobalManager] PokerGameManager spawned");
                }
            }

            Vector3 spawnPos = new Vector3(player.RawEncoded % 2 == 0 ? -3 : 1, 1, 0);
            runner.Spawn(PlayerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new NetworkInputData();
        if (Input.GetKey(KeyCode.W)) myInput.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) myInput.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) myInput.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) myInput.direction += Vector3.right;
        input.Set(myInput);
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

