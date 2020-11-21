using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public int id_team = 0;

    public static NetworkPlayer localPlayer;
    [SyncVar] public string matchID;
    [SyncVar] public int playerIndex;


    NetworkMatchChecker networkMatchChecker;

    GameObject playerLobbyUI;

    [Scene]
    [Tooltip("Assign the gameScene to load for a match")]
    public string gameScene;

    private void Awake()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();  
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            Debug.Log("Spawning other player UI");
            playerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab(this);
        }
    }

    public override void OnStopClient()
    {
        Debug.Log("Client stopped");
        ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log("Client stopped on server");
        ServerDisconnect();
    }

    // HOST TUTORIAL
    public void StartTutorial()
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdStartTutorial(matchID);
    }


    [Command]
    void CmdStartTutorial(string _matchID)
    {
        matchID = _matchID;
        if (MatchMaker.instance.HostGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"Game hosted successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetStartTutorial(true, _matchID, playerIndex);


            MatchMaker.instance.BeginTutorial(matchID);
            Debug.Log($"Game starting...");
        }
        else
        {
            Debug.Log($"Game hosted failed");
            TargetStartTutorial(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetStartTutorial(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.TutorialSuccess(success);
    }


    // HOST GAME
    public void HostGame()
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID);
    }


    [Command]
    void CmdHostGame(string _matchID)
    {
        matchID = _matchID;
        if (MatchMaker.instance.HostGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"Game hosted successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"Game hosted failed");
            TargetHostGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, _matchID);
    }

    // JOIN GAME
    public void JoinGame(string matchID)
    {
        CmdJoinGame(matchID);
    }


    [Command]
    void CmdJoinGame(string _matchID)
    {
        matchID = _matchID;
        if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"Game joined successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"Game joined failed");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID);
    }


    // BEGIN GAME
    public void BeginGame()
    {
        CmdBeginGame();
    }


    [Command]
    void CmdBeginGame()
    {
        MatchMaker.instance.BeginGame(matchID);
        Debug.Log($"Game starting...");

    }

    public void StartGame()
    {
        TargetBeginGame();
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"MatchID: {matchID} | Starting");
        UILobby.instance.Hide();
    }

    // Disconnect Match
    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }

    [Command]
    void CmdDisconnectGame()
    {
        ServerDisconnect();
    }

    void ServerDisconnect()
    {
        MatchMaker.instance.PlayerDisconnect(this.gameObject, matchID);
        networkMatchChecker.matchId = string.Empty.ToGuid();
        id_team = 0;
        playerIndex = 0;
        matchID = string.Empty;
        RpcDisconnectGame();
    }


    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {
        if (playerLobbyUI != null) Destroy(playerLobbyUI);
    }


}
