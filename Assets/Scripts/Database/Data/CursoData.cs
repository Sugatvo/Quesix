using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class CursoData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI nombreCurso = null;
    [SerializeField] TextMeshProUGUI cantidadUsuarios = null;

    [Header("References")]
    [SerializeField] GameEvents events = null;


    private RectTransform _rect = null;
    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return _rect;
        }
    }

    private int _cursoIndex = -1;
    public int CursoIndex { get { return _cursoIndex; } }

    private string id_curso;
    public string cursoID { get { return id_curso; } }

    private string nombre;
    public string Nombre { get { return nombre; } }

    public string[] usersClassroom;

    public void UpdateData(string curso_id, string name, int index)
    {
        nombre = name;
        _cursoIndex = index;
        nombreCurso.text = nombre;
        id_curso = curso_id;
        StartCoroutine(GetUsers(id_curso));
    }
    public void UpdateDataTeacher(string curso_id, string name, int index)
    {
        nombre = name;
        _cursoIndex = index;
        nombreCurso.text = nombre;
        id_curso = curso_id;
        StartCoroutine(GetStudents(id_curso));
    }



    public IEnumerator GetUsers(string id_curso)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_curso", id_curso);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/admin/usersincourse.php", form))
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
                usersClassroom = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.None);

                cantidadUsuarios.text = (usersClassroom.Length - 1).ToString() + " Usuarios";
            }
        }
    }

    public IEnumerator GetStudents(string id_curso)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_curso", id_curso);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/teacher/studentsincourse.php", form))
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
                usersClassroom = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
                cantidadUsuarios.text = (usersClassroom.Length - 1).ToString() + " Alumnos";
            }
        }
    }

    public IEnumerator Refresh()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_curso", id_curso);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/admin/usersincourse.php", form))
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
                usersClassroom = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
                cantidadUsuarios.text = (usersClassroom.Length - 1).ToString() + " Alumnos";
                OnClick();
            }
        }
    }

    public void RefreshUsers()
    {
        StartCoroutine(Refresh());
    }


    public void OnClick()
    {
        events.SelectCurso(usersClassroom, _cursoIndex);
    }

}
