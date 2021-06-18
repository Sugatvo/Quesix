using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UserData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI userInfo = null;
    [SerializeField] TMP_Dropdown cursoDropdown = null;
    [SerializeField] TMP_Dropdown rolDropdown = null;

    Dictionary<string, string> cursos = new Dictionary<string, string>();

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

    private int _userIndex = -1;
    public int UserIndex { get { return _userIndex; } }

    private string id_usuario;
    public string UserID { get { return id_usuario; } }

    private string nombre;
    public string Nombre { get { return nombre; } }

    private string apellido;
    public string Apellido { get { return apellido; } }

    private string rol;
    public string Rol { get { return rol; } }

    private string id_alumno;
    public string StudentID { get { return id_alumno; } }

    private string id_profesor;
    public string TeacherID { get { return id_profesor; } }


    public void UpdateData(string id, string name, string last, string alumno, string profesor, int index)
    {
        id_usuario = id;
        nombre = name;
        apellido = last;
        _userIndex = index;
        userInfo.text = nombre + " " + apellido;
        if (string.IsNullOrEmpty(alumno))
        {
            id_profesor = profesor;
            rol = "Profesor";
        }
        else if (string.IsNullOrEmpty(profesor))
        {
            id_alumno = alumno;
            rol = "Estudiante";
        }
    }

    public void UpdateDataNoAssign(string id, string name, string last, int index)
    {
        id_usuario = id;
        nombre = name;
        apellido = last;
        _userIndex = index;
        userInfo.text = nombre + " " + apellido;
    }

    public void UpdateCursos(string[] cursosString)
    {
        cursoDropdown.options.Clear();
        foreach (string linea in cursosString)
        {
            if (linea != string.Empty)
            {
                string[] data = linea.Split('\t');
                cursoDropdown.options.Add(new TMP_Dropdown.OptionData(data[1]));
                cursos.Add(data[1], data[0]);
            }
        }
    }

    public void UpdateRoles()
    {
        rolDropdown.options.Clear();
        rolDropdown.options.Add(new TMP_Dropdown.OptionData("Profesor"));
        rolDropdown.options.Add(new TMP_Dropdown.OptionData("Estudiante"));
    }


    public void Asignar()
    {
        Debug.Log(rolDropdown.value);
        if(rolDropdown.value == 0)
        {
            //Profesor
            StartCoroutine(SetTeacher(id_usuario, cursos[cursoDropdown.options[cursoDropdown.value].text]));
        }
        else
        {
            //Estudiante
            StartCoroutine(SetStudent(id_usuario, cursos[cursoDropdown.options[cursoDropdown.value].text]));
        }

    }

    public IEnumerator SetTeacher(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", u);
        form.AddField("curso_id", c);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/admin/teacher.php", form))
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
                    Debug.Log("User assign successfully.");
                    AdminManager.Instance.RefreshUsers();
                }
                else
                {
                    Debug.Log("User assign failed. Error #" + text_received);
                }
            }
        }
    }

    public IEnumerator SetStudent(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", u);
        form.AddField("curso_id", c);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/admin/student.php", form))
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
                    Debug.Log("User assign successfully.");
                    AdminManager.Instance.RefreshUsers();
                }
                else
                {
                    Debug.Log("User assign failed. Error #" + text_received);
                }
            }
        }       
    }


    public void Reasignar(int index)
    {
        if (rol.Equals("Profesor"))
        {
            //Profesor
            StartCoroutine(UpdateTeacher(id_profesor, cursos[cursoDropdown.options[cursoDropdown.value].text]));
        }
        else if (rol.Equals("Estudiante"))
        {
            //Estudiante
            StartCoroutine(UpdateStudent(id_alumno, cursos[cursoDropdown.options[cursoDropdown.value].text]));
        }

    }


    public IEnumerator UpdateStudent(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("alumno_id", u);
        form.AddField("curso_id", c);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/admin/updatestudent.php", form))
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
                    Debug.Log("User reassign successfully.");
                    AdminManager.Instance.RefreshUsersClassroom();
                }
                else
                {
                    Debug.Log("User reassign failed. Error #" + text_received);
                }
            }
        }     
    }


    public IEnumerator UpdateTeacher(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("profesor_id", u);
        form.AddField("curso_id", c);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/admin/updateteacher.php", form))
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
                    Debug.Log("User reassign successfully.");
                    AdminManager.Instance.RefreshUsersClassroom();
                }
                else
                {
                    Debug.Log("User reassign failed. Error #" + text_received);
                }
            }
        }       
    }

}
