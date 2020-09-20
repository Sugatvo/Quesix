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

    [SerializeField] Button clasesButton;
    [SerializeField] Button perfilButton;
    [SerializeField] Button logoutButton;

    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI rolInfo;

    [SerializeField] CanvasGroup adminMenu;
    [SerializeField] CanvasGroup assignMenu;
    [SerializeField] CanvasGroup cursosMenu;
    [SerializeField] CanvasGroup studentsInfo;

    [SerializeField] float margins;
    [SerializeField] RectTransform userContentArea;
    [SerializeField] RectTransform cursoContentArea;
    [SerializeField] RectTransform userClassroomContentArea;

    [SerializeField] UserData userPrefab = null;
    [SerializeField] CursoData cursoPrefab = null;
    [SerializeField] UserData userClassroomPrefab = null;


    List<UserData> currentUsers = new List<UserData>();
    List<CursoData> currentCursos = new List<CursoData>();
    List<UserData> currentUsersInClassroom = new List<UserData>();

    public string[] users;
    public string[] cursos;
    private int refreshID;

    [SerializeField] GameEvents events = null;

    // Start is called before the first frame update
    void Start()
    {
        if(DBManager.LoggedIn)
        {
            playerInfo.text = "BIENVENIDO - " + DBManager.nombre + " " + DBManager.apellido;
            rolInfo.text = DBManager.rol;
        }

        events.RefreshUsers += CallAssign;
        events.SelectCurso += ShowUsersClassroom;
        events.RefreshReassign += RefreshUsersClassroom;

        //Obtener cursos
        StartCoroutine(GetCursos());
    }

    public IEnumerator GetCursos()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost/quesix/admin/cursos.php"))
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

    public void CallAssign()
    {
        StartCoroutine(GetUsers());
    }
    public IEnumerator GetUsers()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost/quesix/admin/information.php"))
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
                ShowAssign();
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

            UserData user = (UserData)Instantiate(userPrefab, userContentArea);
            user.UpdateDataNoAssign(data[0], data[1], data[2], i);
            user.UpdateCursos(cursos);
            user.UpdateRoles();
            user.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (user.Rect.sizeDelta.y + margins);
            userContentArea.sizeDelta = new Vector2(userContentArea.sizeDelta.x, offset * -1);

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

            CursoData curso = (CursoData)Instantiate(cursoPrefab, cursoContentArea);
            curso.UpdateData(data[0], data[1], i);
            curso.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (curso.Rect.sizeDelta.y + margins);
            cursoContentArea.sizeDelta = new Vector2(cursoContentArea.sizeDelta.x, offset * -1);

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
            UserData user = (UserData)Instantiate(userClassroomPrefab, userClassroomContentArea);
            user.UpdateData(data[0], data[1], data[2], data[3], data[4], i);
            user.UpdateCursos(cursos);
            user.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (user.Rect.sizeDelta.y + margins);
            userClassroomContentArea.sizeDelta = new Vector2(userClassroomContentArea.sizeDelta.x, offset * -1);

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


    public void ShowAssign()
    {
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        assignMenu.alpha = 1.0f;
        assignMenu.blocksRaycasts = true;
    }

    public void ShowCursos()
    {
        CreateCursos();
        assignMenu.alpha = 0.0f;
        assignMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        cursosMenu.alpha = 1.0f;
        cursosMenu.blocksRaycasts = true; 
    }

    public void ShowUsersClassroom(string[] result, int cursoIndex)
    {
        CreateUsersInClassroom(result);
        assignMenu.alpha = 0.0f;
        assignMenu.blocksRaycasts = false;
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 1.0f;
        studentsInfo.blocksRaycasts = true;
        refreshID = cursoIndex;
    }


    public void RefreshUsersClassroom()
    {
        currentCursos[refreshID].RefreshUsers();
    }

    public void Logout()
    {
        DBManager.LogOut();
        SceneManager.LoadScene(0);
    }


    private void OnDestroy()
    {
        events.RefreshUsers -= CallAssign;
        events.SelectCurso -= ShowUsersClassroom;
    }
}
