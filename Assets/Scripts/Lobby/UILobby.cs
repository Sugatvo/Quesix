using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UILobby : MonoBehaviour
{
    public static UILobby instance;

    [Header("Host/Join")]
    [SerializeField] TMP_InputField joinMatchInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Canvas lobbyCanvas;

    [Header("Lobby")]
    [SerializeField] Transform UIPlayerParent;
    [SerializeField] GameObject UIPlayerPrefab;
    [SerializeField] TextMeshProUGUI matchIDText;
    [SerializeField] GameObject beginGameButton;
    [SerializeField] GameObject fullCanvas;


    [Header("Tutorial")]
    [SerializeField] Button tutorialButton;

    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;


    GameObject playerLobbyUI;

    private void Start()
    {
        instance = this;
    }
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

    public void Host()
    {
        joinMatchInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
        tutorialButton.interactable = false;

        NetworkPlayer.localPlayer.HostGame();
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            if (playerLobbyUI != null) Destroy(playerLobbyUI);
            playerLobbyUI = SpawnPlayerUIPrefab(NetworkPlayer.localPlayer);
            matchIDText.text = matchID;
            beginGameButton.SetActive(true);
        }
        else
        {
            joinMatchInput.interactable = true;
            joinButton.interactable = true;
            hostButton.interactable = true;
            tutorialButton.interactable = true;
        }
    }

    public void Join()
    {
        joinMatchInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
        tutorialButton.interactable = false;

        NetworkPlayer.localPlayer.JoinGame(joinMatchInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            beginGameButton.SetActive(false);
            if (playerLobbyUI != null) Destroy(playerLobbyUI);
            playerLobbyUI = SpawnPlayerUIPrefab(NetworkPlayer.localPlayer);
            matchIDText.text = matchID;
        }
        else
        {
            joinMatchInput.interactable = true;
            joinButton.interactable = true;
            hostButton.interactable = true;
            tutorialButton.interactable = true;
        }
    }

    public GameObject SpawnPlayerUIPrefab(NetworkPlayer player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
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

        lobbyCanvas.enabled = false;
        joinMatchInput.interactable = true;
        joinButton.interactable = true;
        hostButton.interactable = true;
        tutorialButton.interactable = true;
        beginGameButton.SetActive(false);
        joinMatchInput.text = string.Empty;
    }
}
