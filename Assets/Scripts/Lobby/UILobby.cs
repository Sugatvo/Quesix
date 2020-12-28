using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;


public class UILobby : MonoBehaviour
{
    public static UILobby instance;

    /*
    [Header("Host/Join")]
    [SerializeField] TMP_InputField joinMatchInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Canvas lobbyCanvas;*/


    [Header("Lobby")]
    [SerializeField] Transform UITeamsParent;
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

    public void HostSuccess(bool success, string matchID, int id_clase, int selectMethod)
    {
        if (success)
        {
            matchIDText.text = matchID;
            beginGameButton.SetActive(true);
            StartCoroutine(UpdateMatchID(id_clase, matchID));
            StartCoroutine(TeamCreator(id_clase, selectMethod));
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

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/teacher/playclass.php", form))
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

    public IEnumerator TeamCreator(int id_clase, int selectMethod)
    {
        Debug.Log("TeamCreator");
        Debug.Log("id_clase = " + id_clase);
        Debug.Log("selectMethod = "+ selectMethod);
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);
        form.AddField("selectMethod", selectMethod);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/teacher/createteams.php", form))
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
                    Debug.Log("Equipos creados correctamente");
                }

            }
        }
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

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/student/getmatchid.php", form))
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

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            beginGameButton.SetActive(false);
            if (playerLobbyUI != null) Destroy(playerLobbyUI);
            playerLobbyUI = SpawnPlayerUIPrefab(NetworkPlayer.localPlayer);
            matchIDText.text = matchID;
        }
        else
        {
            // Volver a student canvas
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
}
