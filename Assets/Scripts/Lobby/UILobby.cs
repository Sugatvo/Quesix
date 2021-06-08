using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;


public class UILobby : MonoBehaviour
{
    public static UILobby instance;

    [Header("Lobby")]
    [SerializeField] Transform UITeamsParent;
    [SerializeField] UITeam UITeamPrefab;
    [SerializeField] TextMeshProUGUI matchIDText;
    [SerializeField] GameObject beginGameButton;

    [Header("Canvases")]
    [SerializeField] Canvas teacherCanvas;
    [SerializeField] Canvas studentCanvas;
    [SerializeField] Canvas lobbyCanvas;


    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;


    GameObject playerLobbyUI;


    private string matchID;
    List<UITeam> currentTeams = new List<UITeam>();

    public List<Color> team_colors;

    private void Start()
    {
        instance = this;
        team_colors = new List<Color> { 
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
    }

    public void Host(int id_clase, int selectMethod)
    {
        NetworkPlayer.localPlayer.HostGame(id_clase, selectMethod);
    }

    public IEnumerator HostSuccess(bool success)
    {
        if (success)
        {
            yield return new WaitUntil(() => NetworkPlayer.localPlayer.matchID != string.Empty);
            matchIDText.text = NetworkPlayer.localPlayer.matchID;
            beginGameButton.SetActive(true);
            StartCoroutine(UpdateMatchID(NetworkPlayer.localPlayer.clase_id, NetworkPlayer.localPlayer.matchID));
            StartCoroutine(GetTeams(NetworkPlayer.localPlayer.clase_id));
        }
        else
        {
            // Volver a teacher canvas
        }
    }

    public IEnumerator UpdateMatchID(int id_clase, string _matchID)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);
        form.AddField("matchID", _matchID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/playclass.php", form))
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
                if (webRequest.downloadHandler.text.Equals("0"))
                {
                    Debug.Log("Clase actualizada correctamente");
                }

            }
        }
    }

    public IEnumerator GetTeams(int id_clase)
    {
        Debug.Log("GetTeams()");
        EraseTeamUI();
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

                    UITeam UIteam = (UITeam)Instantiate(UITeamPrefab, UITeamsParent);

                    // Setting team ID, name and color
                    UIteam.transform.SetSiblingIndex(team_count - 1);
                    UIteam.SetTeamName("Equipo " + team_count.ToString());
                    int randomIndex = int.Parse(team_information[1]);
                    Debug.Log(randomIndex);
                    UIteam.SetTeamColor(team_colors[randomIndex]);

                    // Setting player 1 information
                    string[] data_player1 = team_information[2].Split('\t');
                    string player1_id = data_player1[0];
                    string player1_nombre = data_player1[1] + " " + data_player1[2];
                    UIteam.SetPlayer1(player1_id, player1_nombre);

                    // Setting player 2 information
                    string[] data_player2 = team_information[3].Split('\t');
                    string player2_id = data_player2[0];
                    string player2_nombre = data_player2[1] + " " + data_player2[2];
                    UIteam.SetPlayer2(player2_id, player2_nombre);

                    currentTeams.Add(UIteam);
                    team_count++;
                }
                SetStatus(NetworkPlayer.localPlayer);
            }
        }
    } 

    void EraseTeamUI()
    {
        foreach (var team in currentTeams)
        {
            Destroy(team.gameObject);
        }
        currentTeams.Clear();
    }

    public void Join(int id_clase)
    {
        //Conseguir match ID
        StartCoroutine(GetMatchID(id_clase));
    }

    public IEnumerator GetMatchID(int id_clase)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/student/getmatchid.php", form))
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
                matchID = webRequest.downloadHandler.text;
                NetworkPlayer.localPlayer.JoinGame(matchID, id_clase);
            }
        }
    }

    public IEnumerator JoinSuccess(bool success)
    {
        if (success)
        {
            yield return new WaitUntil(() => NetworkPlayer.localPlayer.matchID != string.Empty);
            matchIDText.text = NetworkPlayer.localPlayer.matchID;   
            beginGameButton.SetActive(false);
            StartCoroutine(GetTeams(NetworkPlayer.localPlayer.clase_id));
        }
        else
        {
            // Volver a student canvas
        }
    }

    public void SetStatus(NetworkPlayer player)
    {
        StartCoroutine(WaitForTeamCreation(player));
    }
    public IEnumerator WaitForTeamCreation(NetworkPlayer player)
    {
        while(currentTeams.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }
        foreach (var team in currentTeams)
        {
            if (team.isMember(player))
            {
                team.SetPlayerStatus(player);
                break;
            }
        }
    }

    public void OnLobbyDisconnect(NetworkPlayer player)
    {
        foreach (var team in currentTeams)
        {
            if (team.isMember(player))
            {
                team.SetPlayerDisconnect(player);
                break;
            }
        }
        if (player.isLocalPlayer)
        {
            EraseTeamUI();
            if (player.rol.Equals("Profesor"))
            {
                teacherCanvas.gameObject.SetActive(true);
                lobbyCanvas.gameObject.SetActive(false);
            }
            else if (player.rol.Equals("Estudiante"))
            {
                studentCanvas.gameObject.SetActive(true);
                lobbyCanvas.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Error UILobby OnLobbyDisconnect");
            }
        }   
    }

    public void BeginGame()
    {
        NetworkPlayer.localPlayer.BeginGame();
    }

    public void Hide()
    {
        lobbyCanvas.gameObject.SetActive(false);
    }

    public void Show()
    {
        lobbyCanvas.gameObject.SetActive(true);
    }

    public void OnClickSettings()
    {
        settingsCanvasGroup.alpha = 1f;
        settingsCanvasGroup.blocksRaycasts = true;
    }

    public void OnClickCloseSettings()
    {
        settingsCanvasGroup.alpha = 0f;
        settingsCanvasGroup.blocksRaycasts = false;
    }

    public void DisconnectLobby()
    {
        NetworkPlayer.localPlayer.DisconnectLobby();
        beginGameButton.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
