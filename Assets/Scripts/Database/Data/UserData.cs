using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        WWW www = new WWW("http://localhost/quesix/admin/teacher.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);

        if (www.text == "0")
        {
            Debug.Log("User assign successfully.");
            AdminManager.Instance.RefreshUsers();
        }
        else
        {
            Debug.Log("User assign failed. Error #" + www.text);
        }
    }

    public IEnumerator SetStudent(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", u);
        form.AddField("curso_id", c);

        WWW www = new WWW("http://localhost/quesix/admin/student.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);

        if (www.text == "0")
        {
            Debug.Log("User assign successfully.");
            AdminManager.Instance.RefreshUsers();
        }
        else
        {
            Debug.Log("User assign failed. Error #" + www.text);
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
        form.AddField("id_alumno", u);
        form.AddField("curso_id", c);

        WWW www = new WWW("http://localhost/quesix/admin/updatestudent.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);

        if (www.text == "0")
        {
            Debug.Log("User reassign successfully.");
            AdminManager.Instance.RefreshUsersClassroom();
        }
        else
        {
            Debug.Log("User reassign failed. Error #" + www.text);
        }
    }


    public IEnumerator UpdateTeacher(string u, string c)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_profesor", u);
        form.AddField("curso_id", c);

        WWW www = new WWW("http://localhost/quesix/admin/updateteacher.php", form);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.Log(www.error);

        if (www.text == "0")
        {
            Debug.Log("User reassign successfully.");
            AdminManager.Instance.RefreshUsersClassroom();
        }
        else
        {
            Debug.Log("User reassign failed. Error #" + www.text);
        }
    }

}
