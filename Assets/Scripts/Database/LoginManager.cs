using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{

    [SerializeField] CanvasGroup registerForm;
    [SerializeField] CanvasGroup loginForm;
    [SerializeField] CanvasGroup mainMenu;

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

    [Space]

    [Header("Login")]
    public TMP_InputField usernameFieldLogin;
    public TMP_InputField passwordFieldLogin;
    [SerializeField] Button entrarButton;

    public void CallLogin()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameFieldLogin.text);
        form.AddField("password", passwordFieldLogin.text);
        WWW www = new WWW("http://localhost/quesix/login.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);


        if (www.text[0] == '0')
        {
            DBManager.username = usernameFieldLogin.text;
            DBManager.nombre = www.text.Split('\t')[1];
            DBManager.apellido = www.text.Split('\t')[2];
            DBManager.rol = www.text.Split('\t')[3];

            if (DBManager.rol.Equals("Administrador"))
            {
                SceneManager.LoadScene(1);
            }
            else if (DBManager.rol.Equals("Estudiante"))
            {
                SceneManager.LoadScene(2);
            }
            else if (DBManager.rol.Equals("Profesor"))
            {
                SceneManager.LoadScene(3);
            }
            
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

        WWW www = new WWW("http://localhost/quesix/registro.php", form);
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
        }
        else
        {
            usernameInfo.color = new Color32(255, 255, 255, 255);
            usernameInfo.text = "Puedes utilizar letras y números.";
        }
    }


    public void VerifyInputs()
    {
        crearButton.interactable = (usernameField.text.Length >= 6 && passwordField.text.Length >= 3 && repeatField.text.Length >= 3 && nameField.text.Length > 0  && lastField.text.Length > 0 && mailField.text.Length > 0);
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


}
