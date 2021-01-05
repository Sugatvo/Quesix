using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.ComponentModel;

public class AdminManager : MonoBehaviour
{
    [Header("Player information")]
    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI rolInfo;

    [Header("Menus")]
    [SerializeField] CanvasGroup mainMenu;
    [SerializeField] CanvasGroup unassignMenu;
    [SerializeField] CanvasGroup classMenu;
    [SerializeField] CanvasGroup classUsersMenu;

    [Header("Content Area")]
    [SerializeField] RectTransform unassignContentArea;
    [SerializeField] RectTransform classContentArea;
    [SerializeField] RectTransform classUsersContentArea;
    [SerializeField] float margins;

    [Header("Prefabs")]
    [SerializeField] UserData userPrefab = null;
    [SerializeField] CursoData cursoPrefab = null;
    [SerializeField] UserData userClassroomPrefab = null;

    [Header("Canvases")]
    [SerializeField] Canvas loginCanvas;
    [SerializeField] Canvas adminCanvas;

    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;

    List<UserData> currentUsers = new List<UserData>();
    List<CursoData> currentCursos = new List<CursoData>();
    List<UserData> currentUsersInClassroom = new List<UserData>();

    public string[] users;
    public string[] cursos;
    private int refreshID;

    private static AdminManager _instance;
    public static AdminManager Instance { get { return _instance; } }

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

    // Start is called before the first frame update
    void OnEnable()
    {
        if(NetworkPlayer.localPlayer.LoggedIn)
        {
            playerInfo.text = "BIENVENIDO - " + NetworkPlayer.localPlayer.nombre + " " + NetworkPlayer.localPlayer.apellido;
            rolInfo.text = NetworkPlayer.localPlayer.rol;
        }

        //Obtener cursos
        StartCoroutine(GetCursos());
    }

    public IEnumerator GetCursos()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://25.90.9.119/quesix/admin/cursos.php"))
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
                string fulldata = webRequest.downloadHandler.text;
                cursos = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
                Debug.Log(System.String.Format("There are {0} cursos.", cursos.Length - 1));

                foreach (var item in cursos)
                {
                    Debug.Log(item);
                }
            }
        }
    }

    public void RefreshUsers()
    {
        StartCoroutine(GetUsers());
    }
    public IEnumerator GetUsers()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://25.90.9.119/quesix/admin/information.php"))
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
                string fulldata = webRequest.downloadHandler.text;
                users = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
                Debug.Log(System.String.Format("There are {0} users.", users.Length - 1));

                CreateUsers(users);
                ShowUnassignMenu();
            }
        }
    }

    void CreateUsers(string[] result)
    {
        EraseUsers();

        float offset = 0 - margins;
        for (int i = 0; i < result.Length-1; i++)
        {
            string [] data = result[i].Split('\t');

            UserData user = (UserData)Instantiate(userPrefab, unassignContentArea);
            user.UpdateDataNoAssign(data[0], data[1], data[2], i);
            user.UpdateCursos(cursos);
            user.UpdateRoles();
            user.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (user.Rect.sizeDelta.y + margins);
            unassignContentArea.sizeDelta = new Vector2(unassignContentArea.sizeDelta.x, offset * -1);

            currentUsers.Add(user);
        }
    }

    void EraseUsers()
    {
        foreach (var user in currentUsers)
        {
            Destroy(user.gameObject);
        }
        currentUsers.Clear();
    }


    void CreateCursos()
    {
        EraseCursos();

        float offset = 0 - margins;
        for (int i = 0; i < cursos.Length - 1; i++)
        {
            string[] data = cursos[i].Split('\t');

            CursoData curso = (CursoData)Instantiate(cursoPrefab, classContentArea);
            curso.UpdateData(data[0], data[1], i);
            curso.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (curso.Rect.sizeDelta.y + margins);
            classContentArea.sizeDelta = new Vector2(classContentArea.sizeDelta.x, offset * -1);

            currentCursos.Add(curso);
        }
    }

    void EraseCursos()
    {
        foreach (var user in currentCursos)
        {
            Destroy(user.gameObject);
        }
        currentCursos.Clear();
    }


    void CreateUsersInClassroom(string[] result)
    {
        EraseStudents();

        float offset = 0 - margins;
        for (int i = 0; i < result.Length - 1; i++)
        {
            string[] data = result[i].Split('\t');

            Debug.Log(data[0] + data[1] + data[2] + data[3] + data[4]);
            UserData user = (UserData)Instantiate(userClassroomPrefab, classUsersContentArea);
            user.UpdateData(data[0], data[1], data[2], data[3], data[4], i);
            user.UpdateCursos(cursos);
            user.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (user.Rect.sizeDelta.y + margins);
            classUsersContentArea.sizeDelta = new Vector2(classUsersContentArea.sizeDelta.x, offset * -1);

            currentUsersInClassroom.Add(user);
        }
    }

    void EraseStudents()
    {
        foreach (var user in currentUsersInClassroom)
        {
            Destroy(user.gameObject);
        }
        currentUsersInClassroom.Clear();
    }


    public void ShowUnassignMenu()
    {
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        classUsersMenu.alpha = 0.0f;
        classUsersMenu.blocksRaycasts = false;
        unassignMenu.alpha = 1.0f;
        unassignMenu.blocksRaycasts = true;
    }

    public void ShowCursos()
    {
        CreateCursos();
        unassignMenu.alpha = 0.0f;
        unassignMenu.blocksRaycasts = false;
        classUsersMenu.alpha = 0.0f;
        classUsersMenu.blocksRaycasts = false;
        classMenu.alpha = 1.0f;
        classMenu.blocksRaycasts = true; 
    }

    public void SelectCurso(string[] result, int cursoIndex, int id_curso)
    {
        CreateUsersInClassroom(result);
        unassignMenu.alpha = 0.0f;
        unassignMenu.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        classUsersMenu.alpha = 1.0f;
        classUsersMenu.blocksRaycasts = true;
        refreshID = cursoIndex;
    }


    public void RefreshUsersClassroom()
    {
        currentCursos[refreshID].RefreshUsers();
    }

    public void Logout()
    {

        unassignMenu.alpha = 0.0f;
        classMenu.alpha = 0.0f;
        classUsersMenu.alpha = 0.0f;

        unassignMenu.blocksRaycasts = false;
        classMenu.blocksRaycasts = false;
        classUsersMenu.blocksRaycasts = false;

        NetworkPlayer.localPlayer.LogOut();
        loginCanvas.gameObject.SetActive(true);
        adminCanvas.gameObject.SetActive(false);
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
