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
    [SyncVar] public int clase_id;


    public string id_user;
    public string username;
    [SyncVar] public string nombre;
    [SyncVar] public string apellido;
    [SyncVar] public string rol;

    [SyncVar] public string id_admin = string.Empty;
    [SyncVar] public string id_teacher = string.Empty;
    [SyncVar] public string id_student = string.Empty;

    public bool LoggedIn { get { return username != null; } }

    public void LogOut()
    {
        id_user = null;
        username = null;
        nombre = null;
        apellido = null;
        rol = null;
        id_admin = string.Empty;
        id_teacher = string.Empty;
        id_student = string.Empty;
    }


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
            if (rol.Equals("Profesor"))
            {
                Debug.Log("Cliente con rol de profesor, por lo tanto no se instancia el prefab UI");
            }
            else if (rol.Equals("Estudiante"))
            {
                Debug.Log("Spawning other player UI");         
                UILobby.instance.SetStatus(this);
               //playerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab(this);
            }
            else
            {
                Debug.Log("Error en el rol de UIlobby network player");
            }
            
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

    /*
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
    */

    // HOST GAME
    public void HostGame(int id_clase, int selectMethod)
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID, id_clase, selectMethod, nombre, apellido, rol);
    }


    [Command]
    void CmdHostGame(string _matchID, int id_clase, int selectMethod, string _nombre, string _apellido, string _rol)
    {
        matchID = _matchID;
        nombre = _nombre;
        apellido = _apellido;
        clase_id = id_clase;
        rol = _rol;
        if (MatchMaker.instance.HostGame(_matchID, gameObject))
        {
            Debug.Log($"Game hosted successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, id_clase, selectMethod, _nombre , _apellido, _rol);
        }
        else
        {
            Debug.Log($"Game hosted failed");
            TargetHostGame(false, _matchID, id_clase, selectMethod, _nombre, _apellido, _rol);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string _matchID, int id_clase, int selectMethod, string _nombre, string _apellido, string _rol)
    {
        if (success)
        {
            nombre = _nombre;
            apellido = _apellido;
            matchID = _matchID;
            clase_id = id_clase;
            rol = _rol;
        }
        
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, _matchID, id_clase, selectMethod, this);
    }

    // JOIN GAME
    public void JoinGame(string matchID, int id_clase)
    {
        CmdJoinGame(matchID, nombre, apellido, rol, id_clase, id_student);
    }


    [Command]
    void CmdJoinGame(string _matchID, string _nombre, string _apellido, string _rol, int id_clase, string _id_alumno)
    {
        matchID = _matchID;
        nombre = _nombre;
        apellido = _apellido;
        rol = _rol;
        clase_id = id_clase;
        id_student = _id_alumno;
        if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"Game joined successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex, nombre, apellido, rol, id_clase, _id_alumno);
        }
        else
        {
            Debug.Log($"Game joined failed");
            TargetJoinGame(false, _matchID, playerIndex, nombre, apellido, rol, id_clase, _id_alumno);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID, int _playerIndex, string _nombre, string _apellido, string _rol, int id_clase, string _id_alumno)
    {
        if (success)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            nombre = _nombre;
            apellido = _apellido;
            clase_id = id_clase;
            rol = _rol;
            id_student = _id_alumno;
        }   
        Debug.Log($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID, id_clase, this);
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
