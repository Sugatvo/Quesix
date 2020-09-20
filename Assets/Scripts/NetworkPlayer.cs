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

    [Scene]
    [Tooltip("Assign the gameScene to load for a match")]
    public string gameScene;

    private void Start()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();

        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            UILobby.instance.SpawnPlayerUIPrefab(this);
        }
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
        //Camera.main.gameObject.SetActive(false);
        //SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Additive);
    }


}
