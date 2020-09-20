using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class TeacherManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI rolInfo;

    [SerializeField] CanvasGroup cursosMenu;
    [SerializeField] CanvasGroup actionMenu;
    [SerializeField] CanvasGroup studentsInfo;
    [SerializeField] CanvasGroup classMenu;

    [SerializeField] float margins;
    [SerializeField] RectTransform cursoContentArea;
    [SerializeField] RectTransform studentsContentArea;

    [SerializeField] CursoData cursoPrefab = null;
    [SerializeField] StudentData studentPrefab = null;


    List<CursoData> currentCursos = new List<CursoData>();
    List<StudentData> currentStudents = new List<StudentData>();


    private string[] cursos;
    private string[] students;

    [SerializeField] GameEvents events = null;


    // Start is called before the first frame update
    void Start()
    {
        if (DBManager.LoggedIn)
        {
            playerInfo.text = "BIENVENIDO - " + DBManager.nombre + " " + DBManager.apellido;
            rolInfo.text = DBManager.rol;
        }

        events.SelectCurso += ShowActionMenu;

        //Obtener cursos del profesor
        StartCoroutine(GetCursos());
    }

    public void Logout()
    {
        DBManager.LogOut();
        SceneManager.LoadScene(0);
    }

    public IEnumerator GetCursos()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_usuario", DBManager.id_user);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/teacher/teachercourses.php", form))
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

    public void ShowCursos()
    {
        CreateCursos();
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        cursosMenu.alpha = 1.0f;
        cursosMenu.blocksRaycasts = true;
    }

    void CreateCursos()
    {
        EraseCursos();

        float offset = 0 - margins;
        for (int i = 0; i < cursos.Length - 1; i++)
        {
            string[] data = cursos[i].Split('\t');

            CursoData curso = (CursoData)Instantiate(cursoPrefab, cursoContentArea);
            curso.UpdateDataTeacher(data[0], data[1], i);
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


    public void ShowActionMenu (string[] result, int cursoIndex)
    {
        students = result;
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        actionMenu.alpha = 1.0f;
        actionMenu.blocksRaycasts = true;
    }


    public void ShowClassMenu()
    {
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
        classMenu.alpha = 1.0f;
        classMenu.blocksRaycasts = true;
    }



    public void ShowStudents()
    {
        CreateStudents(students);
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
        studentsInfo.alpha = 1.0f;
        studentsInfo.blocksRaycasts = true;
    }


    void CreateStudents(string[] result)
    {
        EraseStudents();

        float offset = 0 - margins;
        for (int i = 0; i < result.Length - 1; i++)
        {
            string[] data = result[i].Split('\t');
            StudentData student = (StudentData)Instantiate(studentPrefab, studentsContentArea);
            student.UpdateData(data[0], data[1], data[2], i);
            student.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (student.Rect.sizeDelta.y + margins);
            studentsContentArea.sizeDelta = new Vector2(studentsContentArea.sizeDelta.x, offset * -1);
            currentStudents.Add(student);
        }
    }

    void EraseStudents()
    {
        foreach (var user in currentStudents)
        {
            Destroy(user.gameObject);
        }
        currentStudents.Clear();
    }

    private void OnDestroy()
    {
        events.SelectCurso -= ShowActionMenu;
    }

}
