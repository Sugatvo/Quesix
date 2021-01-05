using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] CanvasGroup mainMenu;
    [SerializeField] CanvasGroup loginForm;
    [SerializeField] CanvasGroup registerForm;

    [Space]

    [Header("Login")]
    public TMP_InputField usernameFieldLogin;
    public TMP_InputField passwordFieldLogin;
    [SerializeField] Button entrarButton;

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

    public void CallLogin()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameFieldLogin.text);
        form.AddField("password", passwordFieldLogin.text);

        WWW www = new WWW("http://25.90.9.119/quesix/login.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);


        if (www.text[0] == '0')
        {
            NetworkPlayer.localPlayer.username = usernameFieldLogin.text;
            NetworkPlayer.localPlayer.nombre = www.text.Split('\t')[1];
            NetworkPlayer.localPlayer.apellido = www.text.Split('\t')[2];
            NetworkPlayer.localPlayer.rol = www.text.Split('\t')[3];
            NetworkPlayer.localPlayer.id_user = www.text.Split('\t')[4];

            if (NetworkPlayer.localPlayer.rol.Equals("Administrador"))
            {
                loginCanvas.gameObject.SetActive(false);
                adminCanvas.gameObject.SetActive(true);
                NetworkPlayer.localPlayer.id_admin = www.text.Split('\t')[5];

            }
            else if (NetworkPlayer.localPlayer.rol.Equals("Estudiante"))
            {
                loginCanvas.gameObject.SetActive(false);
                studentCanvas.gameObject.SetActive(true);
                NetworkPlayer.localPlayer.id_student = www.text.Split('\t')[5];
            }
            else if (NetworkPlayer.localPlayer.rol.Equals("Profesor"))
            {
                loginCanvas.gameObject.SetActive(false);
                teacherCanvas.gameObject.SetActive(true);
                NetworkPlayer.localPlayer.id_teacher = www.text.Split('\t')[5];
            }

            usernameFieldLogin.text = string.Empty;
            passwordFieldLogin.text = string.Empty;
            ShowMainMenu();
        }
        else
        {
            Debug.Log("Login failed. Error #" + www.text);
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

        WWW www = new WWW("http://25.90.9.119/quesix/register/create.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);


        if (www.text == "0")
        {
            Debug.Log("User created successfully.");
            ShowMainMenu();
        }
        else
        {
            Debug.Log("User creation failed. Error #" + www.text);
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

        WWW www = new WWW("http://25.90.9.119/quesix/register/username_check.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);


        if (www.text == "0")
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
    }


    public void ShowRegister()
    {
        loginForm.alpha = 0.0f;
        loginForm.blocksRaycasts = false;

        mainMenu.alpha = 0.0f;
        mainMenu.blocksRaycasts = false;

        registerForm.alpha = 1.0f;
        registerForm.blocksRaycasts = true;
    }

    public void ShowMainMenu()
    {
        loginForm.alpha = 0.0f;
        loginForm.blocksRaycasts = false;

        mainMenu.alpha = 1.0f;
        mainMenu.blocksRaycasts = true;

        registerForm.alpha = 0.0f;
        registerForm.blocksRaycasts = false;
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
}
