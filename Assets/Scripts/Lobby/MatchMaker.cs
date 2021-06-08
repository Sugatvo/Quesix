using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;


[System.Serializable]
public class Team
{
    public string team_id;
    public string group_index;
    public string player1_id;
    public string player1_nombre;
    public string player2_id;
    public string player2_nombre;
    public Color team_color;
    public int randomIndex;
    public Team(string _team_id, string _group_index, string _player1_id, string _player1_name,
                string _player2_id, string _player2_name, Color _team_color, int _randomIndex) {
        this.team_id = _team_id;
        this.group_index = _group_index;
        this.player1_id = _player1_id;
        this.player1_nombre = _player1_name;
        this.player2_id = _player2_id;
        this.player2_nombre = _player2_name;
        this.team_color = _team_color;
        this.randomIndex = _randomIndex;
    }
    public Team() {}
}


[System.Serializable]
public class Match
{
    public string matchID;
    public int Time;
    public bool isStarted;
    public Scene sceneReference;
    public SyncListTeam equipos = new SyncListTeam();
    public SyncListGameObject players = new SyncListGameObject();
    public GameObject teacher;

    public List<Color> team_colors = new List<Color>{
        new Color32(255, 0, 0, 255),     // Rojo
        new Color32(255, 128, 0, 255),   // Naranjo
        new Color32(0, 255, 0, 255),     // Verde
        new Color32(0, 255, 255, 255),   // Cian
        new Color32(0, 0, 255, 255),     // Azul 
        new Color32(255, 0, 255, 255),   // Magenta
        new Color32(155, 0, 255, 255),   // Morado
        new Color32(255, 255, 0, 255),   // Amarillo
        new Color32(255, 255, 255, 255), // Blanco
        new Color32(128, 128, 128, 255), // Gris
    };

    public Match(string matchID, GameObject _teacher, int mt, bool statement)
    {
        this.matchID = matchID;
        this.Time = mt;
        this.isStarted = statement;
        teacher = _teacher;
    }

    public Match() { }

}

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> { }

[System.Serializable]
public class SyncListTeam : SyncList<Team> { }

[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker instance;

    public SyncListMatch matches = new SyncListMatch();

    public SyncList<string> matchIDs = new SyncList<string>();

    readonly List<Scene> subScenes = new List<Scene>();

    [SerializeField] private CanvasGroup loadingScreenCanvasGroup;
    [SerializeField] private Image _progressBar;


    [Scene]
    public string gameScene;

    [SerializeField]
    [Tooltip("Prefab to use for the camera player game object")]
    protected GameObject cameraPrefab;

    [SerializeField]
    [Tooltip("Prefab to use for the teacher player game object")]
    protected GameObject teacherPrefab;

    [SerializeField]
    [Tooltip("Prefab to use for the robo-raton game object")]
    protected GameObject teamPrefab;

    private int sceneIndex = 1;
    private Transform startPos = null;
    public Dictionary<int, List<NetworkPlayer>> TeamManager = new Dictionary<int, List<NetworkPlayer>>();

    private void Start()
    {
        instance = this;
    }

    public bool check;

    public IEnumerator HostGame(string _matchID, int clase_id, GameObject _player)
    {
        if (!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            Match new_match = new Match(_matchID, _player, 900, false);
            matches.Add(new_match);
            Debug.Log($"Match generated");
            yield return StartCoroutine(TeamCreator(clase_id, new_match));
            check = true;
        }
        else
        {
            Debug.Log($"Match ID already exists");
            check = false;
        }
    }

    public bool JoinGame(string _matchID, GameObject _player)
    {
        if (matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if(matches[i].matchID == _matchID)
                {
                    matches[i].players.Add(_player);
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

        // Loading for teacher
        NetworkPlayer _teacher = _match.teacher.GetComponent<NetworkPlayer>();
        TargetShowLoadingScreen(_teacher.connectionToClient);

        // Loading for students
        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            TargetShowLoadingScreen(_player.connectionToClient);
           
        }
        AsyncOperation loadingMatch = SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        
        while(loadingMatch.progress < 1)
        {
            TargetFillLoadingScreen(_teacher.connectionToClient, loadingMatch.progress);

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

        OnServerReadyQuesix(_teacher.connectionToClient, sceneIndex, _match);
        foreach (var player in _match.players)
        {
            NetworkPlayer _player = player.GetComponent<NetworkPlayer>();
            OnServerReadyQuesix(_player.connectionToClient, sceneIndex, _match);
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

    public void OnServerReadyQuesix(NetworkConnection conn, int sceneIndex, Match _match)
    {
        Debug.Log("OnServerReady NetworkQuesixManager");
        if (conn == null)
        {
            Debug.Log("conn is null");
        }
        else if (conn.identity == null)
        {
            Debug.Log("conn identity is null");
        }
        else if (conn.identity.gameObject == null)
        {
            Debug.Log("conn gameObject is null");
        }
        else if (conn.identity.gameObject.GetComponent<NetworkPlayer>() == null)
        {
            Debug.Log("conn gameObject doesn't have Network Player component");
        }
        else
        {
            Debug.Log("conn is not null, have network identity and have de component NetworkPlayer");
            conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

            NetworkPlayer networkPlayer = conn.identity.gameObject.GetComponent<NetworkPlayer>();

            if (networkPlayer.rol.Equals("Profesor"))
            {
                Debug.Log("Spawning cameraPlayer...");
                GameObject teacherPlayer = Instantiate(teacherPrefab, Vector3.zero, Quaternion.identity);

                teacherPlayer.GetComponent<CameraController>().rol = networkPlayer.rol;

                Debug.Log("Moving cameraPlayer to Scene...");
                SceneManager.MoveGameObjectToScene(teacherPlayer, subScenes[sceneIndex - 1]);

                Debug.Log("Hiding UILobby...");
                networkPlayer.HideUILobby();
                Debug.Log("Hiding LoadingScreen...");
                TargetHideLoadingScreen(conn);

                Debug.Log("Replacing teacher player for connection...");
                NetworkServer.ReplacePlayerForConnection(conn.identity.connectionToClient, teacherPlayer, true);
            }
            else if (networkPlayer.rol.Equals("Estudiante"))
            {
                if (networkPlayer.id_team == 0)
                {
                    foreach (Team team in _match.equipos)
                    {
                        if (networkPlayer.id_student == team.player1_id || networkPlayer.id_student == team.player2_id)
                        {
                            networkPlayer.id_team = int.Parse(team.group_index);

                            // Check if the team was created
                            if (!TeamManager.ContainsKey(networkPlayer.id_team))
                            {
                                List<NetworkPlayer> new_team = new List<NetworkPlayer>();
                                new_team.Add(networkPlayer);
                                TeamManager.Add(networkPlayer.id_team, new_team);
                            }
                            else
                            {
                                // Add networkplayer to the team that was created previously
                                TeamManager[networkPlayer.id_team].Add(networkPlayer);
                            }
                            Debug.Log($"Assigning student {networkPlayer.id_student} to team {team.group_index}");
                            break;
                        }
                    }
                }
                if (CountTeamMembers(TeamManager) == _match.equipos.Count * 2)
                {
                    foreach (List<NetworkPlayer> team in TeamManager.Values)
                    {
                        Debug.Log("Spawning robo-raton..");
                        GameObject gp = SceneLoadedForPlayer(team[0].connectionToClient, team[0], _match);

                        // Spawning cameraPlayer for player1
                        Debug.Log($"Spawning cameraPlayer for student {team[0].id_student}");
                        GameObject cameraPlayer1 = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

                        // Spawning cameraPlayer for player 2
                        Debug.Log($"Spawning cameraPlayer for student {team[1].id_student}");
                        GameObject cameraPlayer2 = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

                        // Moving robo-ratom to Game Scene 
                        Debug.Log("Moving robo-raton and players to GameScene...");
                        SceneManager.MoveGameObjectToScene(gp, subScenes[sceneIndex - 1]);
                        SceneManager.MoveGameObjectToScene(cameraPlayer1, subScenes[sceneIndex - 1]);
                        SceneManager.MoveGameObjectToScene(cameraPlayer2, subScenes[sceneIndex - 1]);


                        // Setting cameraplayer1 information
                        cameraPlayer1.GetComponent<CameraController>().teamObject = gp;
                        cameraPlayer1.GetComponent<TeamManager>().teamObject = gp;
                        cameraPlayer1.GetComponent<TeamManager>().matchID = _match.matchID;
                        cameraPlayer1.GetComponent<TeamManager>().nombre_owner = team[0].nombre;
                        cameraPlayer1.GetComponent<TeamManager>().apellido_owner = team[0].apellido;
                        cameraPlayer1.GetComponent<TeamManager>().nombre_teammate = team[1].nombre;
                        cameraPlayer1.GetComponent<TeamManager>().apellido_teammate = team[1].apellido;
                        cameraPlayer1.GetComponent<TeamManager>().lobbyPlayer = team[0].gameObject;

                        // Setting camera player 2 information
                        cameraPlayer2.GetComponent<CameraController>().teamObject = gp;
                        cameraPlayer2.GetComponent<TeamManager>().teamObject = gp;
                        cameraPlayer2.GetComponent<TeamManager>().matchID = _match.matchID;
                        cameraPlayer2.GetComponent<TeamManager>().nombre_owner = team[1].nombre;
                        cameraPlayer2.GetComponent<TeamManager>().apellido_owner = team[1].apellido;
                        cameraPlayer2.GetComponent<TeamManager>().nombre_teammate = team[0].nombre;
                        cameraPlayer2.GetComponent<TeamManager>().apellido_teammate = team[0].apellido;
                        cameraPlayer2.GetComponent<TeamManager>().lobbyPlayer = team[1].gameObject;

                        // Replacing player 1 connection
                        Debug.Log("Replacing NetworkPlayer to cameraPlayer for player1 connection...");
                        NetworkServer.ReplacePlayerForConnection(team[0].connectionToClient, cameraPlayer1, true);
                        team[0].HideUILobby();
                        TargetHideLoadingScreen(cameraPlayer1.GetComponent<NetworkIdentity>().connectionToClient);

                        // Replacing player 2 connection
                        Debug.Log("Replacing NetworkPlayer to cameraPlayer for player2 connection...");
                        NetworkServer.ReplacePlayerForConnection(team[1].connectionToClient, cameraPlayer2, true);
                        team[1].HideUILobby();
                        TargetHideLoadingScreen(cameraPlayer2.GetComponent<NetworkIdentity>().connectionToClient);

                        // Assign teammates
                        Debug.Log("Assigning teammates");
                        cameraPlayer1.GetComponent<TeamManager>().teammate = cameraPlayer2.GetComponent<NetworkIdentity>();
                        cameraPlayer2.GetComponent<TeamManager>().teammate = cameraPlayer1.GetComponent<NetworkIdentity>();
                    }
                }
            }
            else
            {
                Debug.Log("Error in rol of networkPlayer");
                Debug.Log("rol = " + networkPlayer.rol);
            }
        }
    }
    GameObject SceneLoadedForPlayer(NetworkConnection conn, NetworkPlayer networkPlayer, Match _match)
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

        if (!OnRoomServerSceneLoadedForPlayer(conn, networkPlayer, teamGameObject, _match))
            return null;

        NetworkServer.Spawn(teamGameObject, conn);


        return teamGameObject;

    }

    public bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, NetworkPlayer networkPlayer, GameObject teamGameObject, Match _match)
    {
        PlayerScoreQuesix playerScore = teamGameObject.GetComponent<PlayerScoreQuesix>();
        playerScore.id_team = networkPlayer.id_team;
        List<NetworkIdentity> aux_team = new List<NetworkIdentity>();
        foreach(NetworkPlayer member in TeamManager[networkPlayer.id_team])
        {
            aux_team.Add(member.netIdentity);
        }
        playerScore.equipo = new List<NetworkIdentity>(aux_team);
        playerScore.clase_id = networkPlayer.clase_id;

        foreach (Team team in _match.equipos)
        {
            if (networkPlayer.id_team == int.Parse(team.group_index))
            {
                playerScore.objectColor = team.team_color;
            }
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

            if (_match.teacher != null && _match.teacher.GetComponent<NetworkIdentity>() != null)
            {
                if(_match.teacher.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>() != null)
                {
                    if (_match.teacher.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().isReady)
                    {
                        contador++;
                    }
                }
            }

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
            if (contador == _match.players.Count + 1)
            {
                // Synchronize timer for teacher
                if (_match.teacher != null && _match.teacher.GetComponent<NetworkIdentity>() != null)
                {
                    if (_match.teacher.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>() != null)
                    {
                        _match.teacher.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject.GetComponent<GlobalTimer>().CurrentTime = _match.Time;
                        TargetLocalGlobalTime(_match.teacher.GetComponent<NetworkIdentity>().connectionToClient, _match.Time);
                    }
                }
                // Synchronize timer for students
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


    public void StudentDisconnect(NetworkPlayer student, string _matchID)
    {
        Debug.Log("StudentDisconnect");
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                // Removing student from players
                int playerIndex = matches[i].players.IndexOf(student.gameObject);
                matches[i].players.RemoveAt(playerIndex);
                Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} PlayerScoreQuesix remaining");
                break;
            }
        }

    }

    public void TeacherDisconnect(NetworkPlayer teacher, string _matchID)
    {
        Debug.Log("TeacherDisconnect");
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == _matchID)
            {
                // Removing all students from players
                for(int j = matches[i].players.Count - 1; j >= 0; j--)
                {
                    NetworkPlayer student = matches[i].players[j].GetComponent<NetworkPlayer>();
                    student.networkMatchChecker.matchId = System.Guid.Empty;
                    student.id_team = 0;
                    student.matchID = string.Empty;
                    student.RpcClientDisconnectLobby();
                    matches[i].players.RemoveAt(j);
                    Debug.Log($"Student {student.id_student} disconnected from match {_matchID} | {matches[i].players.Count} PlayerScoreQuesix remaining");
                }

                // Removing teacher from match
                matches[i].teacher = null;

                // Removing Match
                if (matches[i].players.Count == 0 && matches[i].teacher == null)
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
    public IEnumerator TeamCreator(int id_clase, Match _match)
    {
        Debug.Log("TeamCreator()");
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/general/getequipos.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                string[] teams = webRequest.downloadHandler.text.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);
                int team_count = 1;
                foreach (var team in teams)
                {
                    string[] team_information = team.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);
                    // Setting team ID, name and color
                    string team_id = team_information[0];
                    string group_index = team_count.ToString();

                    int randomIndex = int.Parse(team_information[1]);
                    Color team_color = _match.team_colors[randomIndex];
                    _match.team_colors.RemoveAt(randomIndex);

                    // Setting player 1 information
                    string[] data_player1 = team_information[2].Split('\t');
                    string player1_id = data_player1[0];
                    string player1_nombre = data_player1[1] + " " + data_player1[2];

                    // Setting player 2 information
                    string[] data_player2 = team_information[3].Split('\t');
                    string player2_id = data_player2[0];
                    string player2_nombre = data_player2[1] + " " + data_player2[2];

                    Team new_team = new Team(team_id, group_index, player1_id, player1_nombre, player2_id, player2_nombre, team_color, randomIndex);
                    _match.equipos.Add(new_team);
                    team_count++;
                }
            }
        }
    }

    public int CountTeamMembers(Dictionary<int, List<NetworkPlayer>> _TeamManager)
    {
        int contador = 0;
        foreach(List<NetworkPlayer> team in _TeamManager.Values)
        {
            contador += team.Count;
        }
        return contador;
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

