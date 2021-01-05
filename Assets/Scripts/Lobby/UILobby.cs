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
    [SerializeField] GameObject UIPlayerPrefab;
    [SerializeField] TextMeshProUGUI matchIDText;
    [SerializeField] GameObject beginGameButton;
    [SerializeField] GameObject fullCanvas;

    /*
    [Header("Tutorial")]
    [SerializeField] Button tutorialButton;*/

    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;


    GameObject playerLobbyUI;


    private string matchID;
    private string[] teams;
    List<UITeam> currentTeams = new List<UITeam>();

    public List<Color> team_colors = new List<Color>{
        new Color32(56, 162, 251, 255), new Color32(207, 30, 114, 255),
        new Color32(65, 197, 32, 255), new Color32(253, 198, 3, 255)
    };

    private void Start()
    {
        instance = this;
    }

    /*
    public void Tutorial()
    {
        joinMatchInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
        tutorialButton.interactable = false;

        NetworkPlayer.localPlayer.StartTutorial();
    }

    public void TutorialSuccess(bool success)
    {
        if (!success)
        {
            Debug.Log("El tutorial no pudo iniciar");
            joinMatchInput.interactable = true;
            joinButton.interactable = true;
            hostButton.interactable = true;
            tutorialButton.interactable = true;
        }
        else
        {
            Hide();
        }
    }
    */

    public void Host(int id_clase, int selectMethod)
    {
        NetworkPlayer.localPlayer.HostGame(id_clase, selectMethod);
    }

    public void HostSuccess(bool success, string matchID, int id_clase, int selectMethod, NetworkPlayer player)
    {
        if (success)
        {
            matchIDText.text = matchID;
            beginGameButton.SetActive(true);
            StartCoroutine(UpdateMatchID(id_clase, matchID));
            StartCoroutine(TeamCreator(id_clase, player));
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

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://25.90.9.119/quesix/teacher/playclass.php", form))
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

    public IEnumerator TeamCreator(int id_clase, NetworkPlayer player)
    {
        Debug.Log("TeamCreator()");
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://25.90.9.119/quesix/general/getequipos.php", form))
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

                EraseTeamUI();
                teams = webRequest.downloadHandler.text.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);
                int team_count = 1;
                foreach (var team in teams)
                {
                    string[] team_information = team.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);

                    UITeam UIteam = (UITeam)Instantiate(UITeamPrefab, UITeamsParent);
                    UIteam.transform.SetSiblingIndex(team_count - 1);
                    for (int i = 0; i < team_information.Length; i++)
                    {
                        if (i == 0)
                        {
                            UIteam.SetID(int.Parse(team_information[i]));
                            UIteam.SetTeamName("Equipo " + team_count.ToString());

                            int randomIndex = Random.Range(0, team_colors.Count);
                            UIteam.SetTeamColor(team_colors[randomIndex]);
                            team_count++;
                            team_colors.RemoveAt(randomIndex);
                        }
                        else if (i == 1)
                        {
                            string[] data = team_information[i].Split('\t');
                            UIteam.SetPlayer1(int.Parse(data[0]), data[1], data[2]);
                        }
                        else if (i == 2)
                        {
                            string[] data = team_information[i].Split('\t');
                            UIteam.SetPlayer2(int.Parse(data[0]), data[1], data[2]);
                        }
                        else
                        {
                            Debug.Log("Error team creator");
                        }
                    }
                    currentTeams.Add(UIteam);
                }

                if (player.rol.Equals("Estudiante"))
                {
                    SetStatus(player);
                }
                
            }
        }
    }
    void EraseTeamUI()
    {
        foreach (var student in currentTeams)
        {
            Destroy(student.gameObject);
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

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://25.90.9.119/quesix/student/getmatchid.php", form))
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

    public void JoinSuccess(bool success, string matchID, int id_clase, NetworkPlayer player)
    {
        if (success)
        {
            beginGameButton.SetActive(false);
            StartCoroutine(TeamCreator(id_clase, player));
            matchIDText.text = matchID;
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
        while(currentTeams.Count < 0)
        {
            yield return new WaitForEndOfFrame();
        }
        foreach (var team in currentTeams)
        {
            if (team.isMember(player))
            {
                team.SetPlayerStatus(player);
            }
        }
    }


    public GameObject SpawnPlayerUIPrefab(NetworkPlayer player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UITeamsParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
        return newUIPlayer;
    }

    public void BeginGame()
    {
        NetworkPlayer.localPlayer.BeginGame();
    }

    public void Hide()
    {
        fullCanvas.SetActive(false);
    }

    public void Show()
    {
        fullCanvas.SetActive(true);
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
        if(playerLobbyUI != null) Destroy(playerLobbyUI);
        NetworkPlayer.localPlayer.DisconnectGame();

        beginGameButton.SetActive(false);

        /*
        lobbyCanvas.enabled = false;
        joinMatchInput.interactable = true;
        joinButton.interactable = true;
        hostButton.interactable = true;
        tutorialButton.interactable = true;
        joinMatchInput.text = string.Empty;
        */

        // Volver a student or teacher canvas
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
