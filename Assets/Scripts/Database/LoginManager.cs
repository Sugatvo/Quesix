using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] CanvasGroup mainMenu;
    [SerializeField] CanvasGroup loginForm;
    [SerializeField] CanvasGroup registerForm;
    [SerializeField] CanvasGroup loadingCircle;
    [SerializeField] CanvasGroup loginInputs;
    [SerializeField] CanvasGroup errorPopUp;

    [SerializeField] KeyboardSelectableGroup loginSelectableScript;
    [SerializeField] KeyboardSelectableGroup registerSelectableScript;

    [Space]

    [Header("Login")]
    public TMP_InputField usernameFieldLogin;
    public TMP_InputField passwordFieldLogin;
    [SerializeField] Button entrarButton;
    [SerializeField] TextMeshProUGUI errorText;

    [Space]

    [Header("Registro")]
    public TMP_InputField nameField;
    public TMP_InputField lastField;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField repeatField;
    public TMP_InputField mailField;

    [SerializeField] TextMeshProUGUI usernameInfo;
    [SerializeField] Button crearButton;

    [Header("Canvases")]
    [SerializeField] Canvas loginCanvas;
    [SerializeField] Canvas adminCanvas;
    [SerializeField] Canvas teacherCanvas;
    [SerializeField] Canvas studentCanvas;

    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;

    private bool usernameFlag = false;


    private static LoginManager _instance;
    public static LoginManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void CallLogin()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WaitingForResponse();
        yield return new WaitForSeconds(1.0f);

        WWWForm form = new WWWForm();
        form.AddField("username", usernameFieldLogin.text);
        form.AddField("password", passwordFieldLogin.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/login.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string text_received = webRequest.downloadHandler.text;
                if (text_received[0] == '0')
                {
                    string username = usernameFieldLogin.text;
                    string nombre = text_received.Split('\t')[1];
                    string apellido = text_received.Split('\t')[2];
                    string rol = text_received.Split('\t')[3];
                    string id_user = text_received.Split('\t')[4];
                    string id = string.Empty;
                    string status = text_received.Split('\t')[6];

                    Debug.Log("status = " + status);
                    if (status.Equals("0"))
                    {
                        if (rol.Equals("Administrador"))
                        {
                            id = text_received.Split('\t')[5];
                            NetworkPlayer.localPlayer.CmdSetInformation(username, nombre, apellido, rol, id_user, id);
                            StartCoroutine(WaitForSyncUserInfo(loginCanvas.gameObject, adminCanvas.gameObject));
                        }
                        else if (rol.Equals("Estudiante"))
                        {
                            id = text_received.Split('\t')[5];
                            NetworkPlayer.localPlayer.CmdSetInformation(username, nombre, apellido, rol, id_user, id);
                            StartCoroutine(WaitForSyncUserInfo(loginCanvas.gameObject, studentCanvas.gameObject));

                        }
                        else if (rol.Equals("Profesor"))
                        {
                            id = text_received.Split('\t')[5];
                            NetworkPlayer.localPlayer.CmdSetInformation(username, nombre, apellido, rol, id_user, id);
                            StartCoroutine(WaitForSyncUserInfo(loginCanvas.gameObject, teacherCanvas.gameObject));
                        }
                        else
                        {
                            Debug.Log("Error user have no rol");
                        }
                    }
                    else
                    {
                        errorText.text = "Esta cuenta ya está conectada al servidor. Por favor, intente más tarde.";
                        errorPopUp.alpha = 1.0f;
                        ResponseReceived();
                        Debug.Log("Login failed. Error: User is already online");
                    }
                }
                else
                {
                    errorText.text = "Tus credenciales de inicio de sesión no coinciden con ninguna cuenta de nuestro sistema.";
                    errorPopUp.alpha = 1.0f;
                    ResponseReceived();
                    Debug.Log("Login failed. Error #" + text_received);
                }
            }
        }
    }
   

    public void VerifyInputsLogin()
    {
        entrarButton.interactable = (usernameFieldLogin.text.Length > 0  && passwordFieldLogin.text.Length > 0);
    }

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);
        form.AddField("last", lastField.text);
        form.AddField("username", usernameField.text);
        form.AddField("password", passwordField.text);
        form.AddField("mail", mailField.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/register/create.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string text_received = webRequest.downloadHandler.text;
                if (text_received == "0")
                {
                    Debug.Log("User created successfully.");
                    ShowMainMenu();
                }
                else
                {
                    Debug.Log("User creation failed. Error #" + text_received);
                }
            }
        }
    }

    public void VerifyUsername()
    {
        if(usernameField.text.Length < 6 || usernameField.text.Length > 30)
        {
            usernameInfo.color = new Color32(243, 68, 68, 255);
            usernameInfo.text = "Lo sentimos, tu nombre de usuario debe tener entre 6 y 30 caracteres.";
            usernameFlag = false;
            VerifyInputs();
        }
        else
        {
            StartCoroutine(UsernameCheck());  
        }
    }

    IEnumerator UsernameCheck()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameField.text);


        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/register/username_check.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string text_received = webRequest.downloadHandler.text;
                if (text_received == "0")
                {
                    usernameInfo.color = new Color32(255, 255, 255, 255);
                    usernameInfo.text = "Puedes utilizar letras y números.";
                    usernameFlag = true;
                }
                else
                {
                    usernameInfo.color = new Color32(243, 68, 68, 255);
                    usernameInfo.text = "Ese nombre de usuario ya está en uso. Prueba con otro.";
                    usernameFlag = false;
                }
            }
        }
        VerifyInputs();
    }


    public void VerifyInputs()
    {
        crearButton.interactable = (usernameFlag && passwordField.text.Length >= 3 && repeatField.text.Length >= 3 && nameField.text.Length > 0  && lastField.text.Length > 0 && mailField.text.Length > 0);
    }


    public void ShowLogin()
    {
        loginForm.alpha = 1.0f;
        loginForm.blocksRaycasts = true;

        mainMenu.alpha = 0.0f;
        mainMenu.blocksRaycasts = false;

        registerForm.alpha = 0.0f;
        registerForm.blocksRaycasts = false;

        loginSelectableScript.enabled = true;
        registerSelectableScript.enabled = false;
    }


    public void ShowRegister()
    {
        loginForm.alpha = 0.0f;
        loginForm.blocksRaycasts = false;

        mainMenu.alpha = 0.0f;
        mainMenu.blocksRaycasts = false;

        registerForm.alpha = 1.0f;
        registerForm.blocksRaycasts = true;

        loginSelectableScript.enabled = false;
        registerSelectableScript.enabled = true;
    }

    public IEnumerator WaitForSyncUserInfo(GameObject loginCanvas, GameObject rolCanvas)
    {
        yield return new WaitUntil(() => NetworkPlayer.localPlayer.id_user != string.Empty);
        yield return StartCoroutine(SetStatus(true));
        ResponseReceived();
        loginCanvas.SetActive(false);
        rolCanvas.SetActive(true);
        // Reset login
        usernameFieldLogin.text = string.Empty;
        passwordFieldLogin.text = string.Empty;
        ShowMainMenu();
    }


    public IEnumerator SetStatus(bool status)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", NetworkPlayer.localPlayer.id_user);
        form.AddField("status", status.ToString());

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/general/setstatus.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string text_received = webRequest.downloadHandler.text;
                Debug.Log(text_received);
                if (text_received == "0")
                {
                    Debug.Log("status set to: " + status);

                }
            }
        }
    }

    public void ShowMainMenu()
    {
        loginForm.alpha = 0.0f;
        loginForm.blocksRaycasts = false;

        mainMenu.alpha = 1.0f;
        mainMenu.blocksRaycasts = true;

        registerForm.alpha = 0.0f;
        registerForm.blocksRaycasts = false;

        // Reset login
        usernameFieldLogin.text = string.Empty;
        passwordFieldLogin.text = string.Empty;

        HidePopUp();
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

    public void WaitingForResponse()
    {
        entrarButton.interactable = false;
        loadingCircle.alpha = 1.0f;
        loginInputs.alpha = 0.0f;
        loginInputs.blocksRaycasts = false;
        loginInputs.interactable = false;
    }

    public void ResponseReceived()
    {
        entrarButton.interactable = true;
        loadingCircle.alpha = 0.0f;
        loginInputs.alpha = 1.0f;
        loginInputs.blocksRaycasts = true;
        loginInputs.interactable = true;
    }

    public void HidePopUp()
    {
        if(errorPopUp.alpha > 0f)
        {
            errorPopUp.alpha = 0.0f;
        }
    }
}
