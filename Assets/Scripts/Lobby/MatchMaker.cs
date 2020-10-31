using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Match
{
    public string matchID;
    public int MaxTime;
    public bool isStarted;
    public SyncListGameObject players = new SyncListGameObject();

    public Match(string matchID, GameObject player, int mt, bool statement)
    {
        this.matchID = matchID;
        this.MaxTime = mt;
        this.isStarted = statement;
        players.Add(player);
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

    [SerializeField] GameObject turnManagerPrefab;

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

    private int group_index = 1;

    private int sceneIndex = 1;

    private Transform startPos = null;

    public Material baseRosa;
    public Material baseVerde;

    private void Start()
    {
        instance = this;
    }

    public bool HostGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            matches.Add(new Match(_matchID, _player, 900, false));
            Debug.Log($"Match generated");
            playerIndex = 1;
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
        yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        subScenes.Add(SceneManager.GetSceneAt(sceneIndex));

        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            OnServerReady(player.GetComponent<NetworkIdentity>().connectionToClient, sceneIndex);
            _player.StartGame();
        }
        _match.isStarted = true;
        sceneIndex ++;
        SyncGlobalTimer(_match);
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

    public void OnServerReady(NetworkConnection conn, int sceneIndex)
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
                    team_ids.Add(conn.connectionId);
                    team_aux.Add(conn.identity);
                }

                if (team_ids.Count == 2)
                {
                    GameObject gp = SceneLoadedForPlayer(conn, networkPlayer);

                    for (int i = 0; i < 2; i++)
                    {
                        GameObject cameraPlayer = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

                        cameraPlayer.GetComponent<CameraController>().teamObject = gp;
                        cameraPlayer.GetComponent<TeamManager>().teamObject = gp;

                        if (i == 0)
                        {
                            cameraPlayer.GetComponent<TeamManager>().teammate = team_aux[1];
                            cameraPlayer.GetComponent<TeamManager>().ownerID = team_ids[0];
                            cameraPlayer.GetComponent<TeamManager>().teammateID = team_ids[1];
                            cameraPlayer.GetComponent<CameraController>().type = 1;
                        }
                        else
                        {
                            cameraPlayer.GetComponent<TeamManager>().teammate = team_aux[0];
                            cameraPlayer.GetComponent<TeamManager>().ownerID = team_ids[1];
                            cameraPlayer.GetComponent<TeamManager>().teammateID = team_ids[0];
                            cameraPlayer.GetComponent<CameraController>().type = 2;
                        }
                        NetworkServer.ReplacePlayerForConnection(team_aux[i].connectionToClient, cameraPlayer, true);
                        SceneManager.MoveGameObjectToScene(cameraPlayer, subScenes[sceneIndex-1]);
                    }
                    SceneManager.MoveGameObjectToScene(gp, subScenes[sceneIndex - 1]);
                    team_ids = new List<int>();
                    team_aux = new List<NetworkIdentity>();
                    group_index += 1;
                }

            }
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

        Vector2 st_pos;
        st_pos.x = networkPlayer.transform.position.x;
        st_pos.y = networkPlayer.transform.position.y;
        playerScore.start_pos = st_pos;

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
        while (_match.isStarted && _match.MaxTime > 0)
        {
            int contador = 0;
            foreach (var player in _match.players)
            {
                if (player.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().isReady)
                {
                    contador++;
                }
            }
            // Verify all players are ready
            if (contador == _match.players.Count)
            {
                foreach (var player in _match.players)
                {
                    TargetLocalGlobalTime(player.GetComponent<NetworkIdentity>().connectionToClient, _match.MaxTime);
                }

                _match.MaxTime--;
            }
            
            yield return new WaitForSeconds(1.0f);

        }
        
    }


    [TargetRpc]
    public void TargetLocalGlobalTime(NetworkConnection target, int mt)
    {
        target.identity.gameObject.GetComponent<GlobalTimer>().SetMaxTime(mt);
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

