using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Match
{
    public string matchID;
    public int Time;
    public bool isStarted;
    public Scene sceneReference;
    public SyncListGameObject players = new SyncListGameObject();
    public GameObject teacher;

    public Match(string matchID, GameObject _teacher, int mt, bool statement)
    {
        this.matchID = matchID;
        this.Time = mt;
        this.isStarted = statement;
        teacher = _teacher;
        // players.Add(player);
    }

    public Match() { }

}

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> { }


[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker instance;

    public SyncListMatch matches = new SyncListMatch();

    public SyncListString matchIDs = new SyncListString();

    readonly List<Scene> subScenes = new List<Scene>();

    [SerializeField] private CanvasGroup loadingScreenCanvasGroup;
    [SerializeField] private Image _progressBar;

    [Scene]
    public string gameScene;

    [SerializeField]
    [Tooltip("Prefab to use for the camera player game object")]
    protected GameObject cameraPrefab;

    [SerializeField]
    [Tooltip("Prefab to use for the camera player game object")]
    protected GameObject teamPrefab;

    public List<int> team_ids = new List<int>();
    public List<NetworkIdentity> team_aux = new List<NetworkIdentity>();
    public List<string> nombres = new List<string>();
    public List<string> apellidos = new List<string>();

    private int group_index = 1;

    private int sceneIndex = 1;

    private Transform startPos = null;

    public Material baseRosa;
    public Material baseVerde;

    private void Start()
    {
        instance = this;
    }

    public bool HostGame(string _matchID, GameObject _player)
    {
        if (!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            matches.Add(new Match(_matchID, _player, 900, false));
            Debug.Log($"Match generated");
            return true;
        }
        else
        {
            Debug.Log($"Match ID already exists");
            return false;
        }
        
    }

    public bool JoinGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if (matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if(matches[i].matchID == _matchID)
                {
                    matches[i].players.Add(_player);
                    playerIndex = matches[i].players.Count;

                    break;
                }
            }
            Debug.Log($"Match joined");
            return true;
        }
        else
        {
            Debug.Log($"Match ID does not exists");
            return false;
        }

    }

    public void BeginGame(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                StartCoroutine(LoadScene(matches[i]));
                break;
            }
        }

    }


    IEnumerator LoadScene(Match _match) {

        // Loading 
        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            TargetShowLoadingScreen(_player.connectionToClient);
           
        }
        AsyncOperation loadingMatch = SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        
        while(loadingMatch.progress < 1)
        {
            foreach (var player in _match.players)
            {
                NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
                TargetFillLoadingScreen(_player.connectionToClient, loadingMatch.progress);
            }
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("sceneIndex = " + sceneIndex);
        subScenes.Add(SceneManager.GetSceneAt(sceneIndex));
        _match.sceneReference = SceneManager.GetSceneAt(sceneIndex);
        group_index = 1;


        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            OnServerReady(player.GetComponent<NetworkIdentity>().connectionToClient, sceneIndex, _match.matchID, _player.playerIndex);
            _player.StartGame();
            TargetHideLoadingScreen(_player.connectionToClient);
        }
        yield return new WaitForEndOfFrame();

        _match.isStarted = true;
        sceneIndex ++;
        SyncGlobalTimer(_match);
    }


    [TargetRpc]
    void TargetShowLoadingScreen(NetworkConnection target)
    {
        loadingScreenCanvasGroup.alpha = 1f;
    }


    [TargetRpc]
    void TargetFillLoadingScreen(NetworkConnection target, float value)
    {
        _progressBar.fillAmount = value;
    }

    [TargetRpc]
    void TargetHideLoadingScreen(NetworkConnection target)
    {
        loadingScreenCanvasGroup.alpha = 0f;
    }

    public static string GetRandomMatchID()
    {
        string _id = string.Empty;

        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);

            if(random < 26)
            {
                _id += (char)(random + 65);
            }
            else
            {
                _id += (random - 26).ToString();
            }
        }
        Debug.Log($"Random Match ID: {_id}");
        return _id;
    }

    public void OnServerReady(NetworkConnection conn, int sceneIndex, string _matchID, int _playerIndex)
    {
        Debug.Log("OnServerReady NetworkQuesixManager");
        if (conn.identity == null)
        {
            Debug.Log("Ready with no player object");
        }

        Debug.Log("SubScenes");
        foreach (var item in subScenes)
        {
            Debug.Log(item);
        }

        conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

        if (conn != null && conn.identity != null)
        {
            GameObject networkPlayer = conn.identity.gameObject;

            // if null or not a room player, dont replace it
            if (networkPlayer != null && networkPlayer.GetComponent<NetworkPlayer>() != null)
            {
                Debug.Log("conn is not null, have network identity and have de component NetworkPlayer");

                if (networkPlayer.GetComponent<NetworkPlayer>().id_team == 0)
                {
                    networkPlayer.GetComponent<NetworkPlayer>().id_team = group_index;
                    team_ids.Add(conn.connectionId);
                    team_aux.Add(conn.identity);
                    nombres.Add(networkPlayer.GetComponent<NetworkPlayer>().nombre);
                    apellidos.Add(networkPlayer.GetComponent<NetworkPlayer>().apellido);
                }

                if (team_ids.Count == 2)
                {
                    Debug.Log("Spawning robo-raton..");
                    GameObject gp = SceneLoadedForPlayer(conn, networkPlayer);

                    for (int i = 0; i < 2; i++)
                    {
                        Debug.Log("Spawning cameraPlayer...");
                        GameObject cameraPlayer = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

                        cameraPlayer.GetComponent<CameraController>().teamObject = gp;
                        cameraPlayer.GetComponent<TeamManager>().teamObject = gp;
                        cameraPlayer.GetComponent<TeamManager>().matchID = _matchID;
                        if (i == 0)
                        {
                            cameraPlayer.GetComponent<TeamManager>().nombre_owner = nombres[0];
                            cameraPlayer.GetComponent<TeamManager>().apellido_owner = apellidos[0];
                            cameraPlayer.GetComponent<TeamManager>().nombre_teammate = nombres[1];
                            cameraPlayer.GetComponent<TeamManager>().apellido_teammate = apellidos[1];
                            cameraPlayer.GetComponent<TeamManager>().teammate = team_aux[1];
                            cameraPlayer.GetComponent<TeamManager>().ownerID = team_ids[0];
                            cameraPlayer.GetComponent<TeamManager>().teammateID = team_ids[1];
                            cameraPlayer.GetComponent<CameraController>().type = 1;
                        }
                        else
                        {
                            cameraPlayer.GetComponent<TeamManager>().nombre_owner = nombres[1];
                            cameraPlayer.GetComponent<TeamManager>().apellido_owner = apellidos[1];
                            cameraPlayer.GetComponent<TeamManager>().nombre_teammate = nombres[0];
                            cameraPlayer.GetComponent<TeamManager>().apellido_teammate = apellidos[0];
                            cameraPlayer.GetComponent<TeamManager>().teammate = team_aux[0];
                            cameraPlayer.GetComponent<TeamManager>().ownerID = team_ids[1];
                            cameraPlayer.GetComponent<TeamManager>().teammateID = team_ids[0];
                            cameraPlayer.GetComponent<CameraController>().type = 2;
                        }
                        cameraPlayer.GetComponent<TeamManager>().lobbyPlayer = team_aux[i].gameObject;

                        Debug.Log("ReplacePlayerForConnection...");
                        NetworkServer.ReplacePlayerForConnection(team_aux[i].connectionToClient, cameraPlayer, true);
                        SceneManager.MoveGameObjectToScene(cameraPlayer, subScenes[sceneIndex-1]);
                    }
                    SceneManager.MoveGameObjectToScene(gp, subScenes[sceneIndex - 1]);
                    team_ids = new List<int>();
                    team_aux = new List<NetworkIdentity>();
                    nombres = new List<string>();
                    apellidos = new List<string>();
                    group_index += 1;
                }

            }
            else
            {
                Debug.Log("Networkplayer component or object is null");
            }
        }
        else
        {
            Debug.Log("conn or conn.identity are null");
        }
    }
    GameObject SceneLoadedForPlayer(NetworkConnection conn, GameObject networkPlayer)
    {
        GameObject teamGameObject = null;
        if (teamGameObject == null)
        {
            int index = UnityEngine.Random.Range(0, NetworkManager.startPositions.Count);
            startPos = NetworkManager.startPositions[index];
            NetworkManager.startPositions.RemoveAt(index);

            teamGameObject = startPos != null
                ? Instantiate(teamPrefab, startPos.position, startPos.rotation)
                : Instantiate(teamPrefab, Vector3.zero, Quaternion.identity);

           
            if (startPos.position.z == 9)
            {
                // Left
                teamGameObject.transform.eulerAngles = new Vector3(teamGameObject.transform.eulerAngles.x, -90.0f, teamGameObject.transform.eulerAngles.z);
            }
            else if (startPos.position.z ==  -9)
            {
                // Right
                teamGameObject.transform.eulerAngles = new Vector3(teamGameObject.transform.eulerAngles.x, 90.0f, teamGameObject.transform.eulerAngles.z);
            }
            else if(startPos.position.x == -9)
            {
                // Bot
                teamGameObject.transform.eulerAngles = new Vector3(teamGameObject.transform.eulerAngles.x, 180.0f, teamGameObject.transform.eulerAngles.z);
            }
            else
            {
                // Top
                teamGameObject.transform.eulerAngles = new Vector3(teamGameObject.transform.eulerAngles.x, 0.0f, teamGameObject.transform.eulerAngles.z);
            }
        }

        if (!OnRoomServerSceneLoadedForPlayer(conn, networkPlayer, teamGameObject))
            return null;

        NetworkServer.Spawn(teamGameObject, conn);


        return teamGameObject;

    }

    public bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject networkPlayer, GameObject teamGameObject)
    {
        PlayerScoreQuesix playerScore = teamGameObject.GetComponent<PlayerScoreQuesix>();
        playerScore.id_team = networkPlayer.GetComponent<NetworkPlayer>().id_team;
        playerScore.equipo = new List<NetworkIdentity>(team_aux);
        playerScore.clase_id = networkPlayer.GetComponent<NetworkPlayer>().clase_id;

        Debug.Log("networkPlayer.id_team = " + networkPlayer.GetComponent<NetworkPlayer>().id_team);

        if (playerScore.id_team == 1)
        {
            playerScore.objectColor = new Color(0.52f, 0.16f, 0.9f);
            playerScore.emissionColor = new Color(0.0951f, 0.0431f, 0.2f);
        }

        if (playerScore.id_team == 2)
        {
            playerScore.objectColor = new Color(0.38f, 0.91f, 0.19f);
            playerScore.emissionColor = new Color(0.039f, 0.2f, 0.043f);
        }

        return true;
    }

    [ServerCallback]
    public void SyncGlobalTimer(Match _match)
    {
        if (_match.isStarted)
        {
            StartCoroutine(StartGlobalTimer(_match));
        }
    }

    IEnumerator StartGlobalTimer(Match _match)
    {
        // Setting Max Time of the Match
        foreach (var player in _match.players)
        {
            if (player != null && player.GetComponent<NetworkIdentity>() != null)
            {
                if (player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>() != null)
                {
                    player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().MaxTime = _match.Time;
                    TargetSetMaxTime(player.GetComponent<NetworkIdentity>().connectionToClient, _match.Time);
                }
            }

        }
        while (_match.isStarted && _match.Time > 0)
        {
            int contador = 0;
            foreach (var player in _match.players)
            {
                if(player != null && player.GetComponent<NetworkIdentity>() != null)
                {
                    if (player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>() != null)
                    {
                        if (player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().isReady)
                        {
                            contador++;
                        }
                    }
                }
               
            }
            // Verify all players are ready
            if (contador == _match.players.Count)
            {
                foreach (var player in _match.players)
                {
                    if (player != null && player.GetComponent<NetworkIdentity>() != null)
                    {
                        if (player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>() != null)
                        {
                            player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().CurrentTime = _match.Time;
                            TargetLocalGlobalTime(player.GetComponent<NetworkIdentity>().connectionToClient, _match.Time);
                        }
                    }
                }
                _match.Time--;
            }
            
            yield return new WaitForSeconds(1.0f);

        }
        
    }


    [TargetRpc]
    public void TargetLocalGlobalTime(NetworkConnection target, int mt)
    {
        target.identity.gameObject.GetComponent<GlobalTimer>().SetTime(mt);
    }

    [TargetRpc]
    public void TargetSetMaxTime(NetworkConnection target, int mt)
    {
        target.identity.gameObject.GetComponent<GlobalTimer>().MaxTime = mt;
    }


    public void BeginTutorial(string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                StartCoroutine(LoadSceneTutorial(matches[i]));
                break;
            }
        }
    }

    IEnumerator LoadSceneTutorial(Match _match)
    {
        yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        subScenes.Add(SceneManager.GetSceneAt(sceneIndex));
        group_index = 1;
        _match.sceneReference = SceneManager.GetSceneAt(sceneIndex);

        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            OnServerReadyTutorial(player.GetComponent<NetworkIdentity>().connectionToClient, sceneIndex);
        }
        _match.isStarted = true;
        sceneIndex++;
        SyncGlobalTimer(_match);
    }

    public void OnServerReadyTutorial(NetworkConnection conn, int sceneIndex)
    {
        Debug.Log("OnServerReady NetworkQuesixManager");
        if (conn.identity == null)
        {
            Debug.Log("Ready with no player object");
        }

        Debug.Log("SubScenes");
        foreach (var item in subScenes)
        {
            Debug.Log(item);
        }

        conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

        if (conn != null && conn.identity != null)
        {
            GameObject networkPlayer = conn.identity.gameObject;

            // if null or not a room player, dont replace it
            if (networkPlayer != null && networkPlayer.GetComponent<NetworkPlayer>() != null)
            {

                if (networkPlayer.GetComponent<NetworkPlayer>().id_team == 0)
                {
                    networkPlayer.GetComponent<NetworkPlayer>().id_team = group_index;
                }

                GameObject gp = SceneLoadedForPlayer(conn, networkPlayer);

                GameObject cameraPlayer = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

                cameraPlayer.GetComponent<CameraController>().teamObject = gp;
                cameraPlayer.GetComponent<TeamManager>().teamObject = gp;
                cameraPlayer.GetComponent<TeamManager>().ownerID = conn.connectionId;
                cameraPlayer.GetComponent<CameraController>().type = 1;
                cameraPlayer.GetComponent<GlobalTimer>().isTutorial = true;

                NetworkServer.ReplacePlayerForConnection(conn, cameraPlayer, true);

                SceneManager.MoveGameObjectToScene(cameraPlayer, subScenes[sceneIndex - 1]);
                SceneManager.MoveGameObjectToScene(gp, subScenes[sceneIndex - 1]);

                TargetSetTutorial(conn, cameraPlayer);
            }
        }
    }

    [TargetRpc]
    public void TargetSetTutorial(NetworkConnection target, GameObject _cameraPlayer)
    {
        _cameraPlayer.GetComponent<GlobalTimer>().SetTutorial(true);
    }

    public void PlayerDisconnect(GameObject player, string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                int playerIndex = matches[i].players.IndexOf(player);
                matches[i].players.RemoveAt(playerIndex);
                Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} PlayerScoreQuesix remaining");

                if (matches[i].players.Count == 0)
                {
                    if (matches[i].sceneReference != null)
                    {
                        StartCoroutine(UnloadScene(matches[i].sceneReference.path));
                        matches[i].isStarted = false;
                    }

                    Debug.Log($"No more players in Match. Terminating");
                    matches.RemoveAt(i);
                    matchIDs.Remove(_matchID); 
                   
                }
                UILobby.instance.Show();
                break;
            }
        }

    }


    public void PlayerDisconnectFromGame(GameObject networkPlayer, string _matchID, GameObject cameraPlayer)
    {
        Debug.Log("PlayerDisconnectFromGame");
        Debug.Log(networkPlayer);
        Debug.Log(_matchID);
        Debug.Log(cameraPlayer);

        NetworkIdentity networkIdentity = cameraPlayer.gameObject.GetComponent<NetworkIdentity>(); 

        if(NetworkServer.ReplacePlayerForConnection(cameraPlayer.GetComponent<NetworkIdentity>().connectionToClient, networkPlayer, true))
        {
            Debug.Log("ReplacePlayerForConnection is true");
        }
        else
        {
            Debug.Log("ReplacePlayerForConnection is false");
        }
        
        NetworkServer.SendToClientOfPlayer(networkIdentity, new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.UnloadAdditive });
    }

    IEnumerator UnloadScene(string scenePath)
    {
        subScenes.Remove(SceneManager.GetSceneByPath(scenePath));
        if (SceneManager.GetSceneByPath(scenePath).IsValid())
        {
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath(scenePath));
            Debug.Log("Server UnloadScene completed");
            sceneIndex--;
            Debug.Log("SceneIndex = " + sceneIndex);
        }
        
    }


}

public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}

