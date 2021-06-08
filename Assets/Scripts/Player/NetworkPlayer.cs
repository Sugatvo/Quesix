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
    [SyncVar] public int clase_id;

    [SyncVar] public string id_user;
    [SyncVar] public string username;
    [SyncVar] public string nombre;
    [SyncVar] public string apellido;
    [SyncVar] public string rol;

    [SyncVar] public string id_admin = string.Empty;
    [SyncVar] public string id_teacher = string.Empty;
    [SyncVar] public string id_student = string.Empty;

    public bool LoggedIn { get { return username != null; } }

    public void LogOut()
    {
        StartCoroutine(WaitForStatus());
       
    }
    IEnumerator WaitForStatus()
    {
        yield return StartCoroutine(LoginManager.Instance.SetStatus(false));
        id_user = null;
        username = null;
        nombre = null;
        apellido = null;
        rol = null;
        id_admin = string.Empty;
        id_teacher = string.Empty;
        id_student = string.Empty;
    }

    [Command]
    public void CmdSetInformation(string _username, string _nombre, string _apellido, string _rol, string _id_user, string _id)
    {
        this.username = _username;
        this.nombre = _nombre;
        this.apellido = _apellido;
        this.rol = _rol;
        this.id_user = _id_user;

        if (_rol.Equals("Administrador"))
        {
            this.id_admin = _id;

        }
        else if (_rol.Equals("Estudiante"))
        {
            this.id_student = _id;
        }
        else if (_rol.Equals("Profesor"))
        {
            this.id_teacher = _id;
        }
    }

    public NetworkMatchChecker networkMatchChecker;


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
                Debug.Log("Set other player status");         
                UILobby.instance.SetStatus(this);
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
        ClientDisconnectLobby();
        LogOut();
        
    }

    public override void OnStopServer()
    {
        Debug.Log("Client stopped on server");
        ServerDisconnectLobby();
    }

    // HOST GAME
    public void HostGame(int id_clase, int selectMethod)
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID, id_clase, selectMethod);
    }


    [Command]
    void CmdHostGame(string _matchID, int id_clase, int selectMethod)
    {
        matchID = _matchID;
        clase_id = id_clase;
        StartCoroutine(WaitForHostGame(matchID, clase_id, selectMethod));
    }

    IEnumerator WaitForHostGame(string _matchID, int id_clase, int selectMethod)
    {
        yield return StartCoroutine(MatchMaker.instance.HostGame(_matchID, id_clase, gameObject));
        if (MatchMaker.instance.check)
        {
            Debug.Log($"Game hosted successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true);
        }
        else
        {
            Debug.Log($"Game hosted failed");
            TargetHostGame(false);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success)
    {
        StartCoroutine(UILobby.instance.HostSuccess(success));
    }

    // JOIN GAME
    public void JoinGame(string matchID, int id_clase)
    {
        CmdJoinGame(matchID, id_clase);
    }

    [Command]
    void CmdJoinGame(string _matchID, int id_clase)
    {
        matchID = _matchID;
        clase_id = id_clase;
        Debug.Log("CmdJoinGame....");
        Debug.Log("matchID = " + matchID);
        Debug.Log("clase_id = " + clase_id);
        if (MatchMaker.instance.JoinGame(_matchID, gameObject))
        {
            Debug.Log($"Game joined successfully");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true);
        }
        else
        {
            Debug.Log($"Game joined failed");
            TargetJoinGame(false);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success)
    {
        StartCoroutine(UILobby.instance.JoinSuccess(success));
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

    public void HideUILobby()
    {
        TargetHideUILobby();
    }

    [TargetRpc]
    void TargetHideUILobby()
    {
        Debug.Log($"MatchID: {matchID} | Starting");
        UILobby.instance.Hide();
    }

    // Disconnect Match
    public void DisconnectLobby()
    {
        CmdDisconnectLobby();
    }

    [Command]
    void CmdDisconnectLobby()
    {
        ServerDisconnectLobby();
    }

    void ServerDisconnectLobby()
    {
        if (rol.Equals("Profesor"))
        {
            MatchMaker.instance.TeacherDisconnect(this, matchID);
            networkMatchChecker.matchId = System.Guid.Empty;
            id_team = 0;
            matchID = string.Empty;
            RpcClientDisconnectLobby();
        }
        else if (rol.Equals("Estudiante"))
        {
            MatchMaker.instance.StudentDisconnect(this, matchID);
            networkMatchChecker.matchId = System.Guid.Empty;
            id_team = 0;
            matchID = string.Empty;
            RpcClientDisconnectLobby();
        }
        else
        {
            Debug.Log("Error en rol NetworkPlayer Server Disconnect");
        }
    }

    [ClientRpc]
    public void RpcClientDisconnectLobby()
    {
        ClientDisconnectLobby();
    }

    void ClientDisconnectLobby()
    {
        UILobby.instance.OnLobbyDisconnect(this);
    }

}
