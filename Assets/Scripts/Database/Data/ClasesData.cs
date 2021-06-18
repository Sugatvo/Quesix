using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class ClasesData : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI nombreClase;
    [SerializeField] TextMeshProUGUI fechaClase;
    [SerializeField] TextMeshProUGUI estadoClase;
    [SerializeField] Button joinButton;

    [Header("Classes Data")]
    public int id_clase;
    public int curso_id;
    public string fecha;
    public int tiempo_maximo;
    public int estado;
    public string codigo;
    public int selectMethod;
    public string nombre;
    public string tema;
    public string dificultad;
    public string[] materias;

    [Header("Reference")]
    public int index;

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

    public void UpdateData(int idClase, int cursoId, string _fecha, int tiempoMaximo, int _estado, string _codigo, int _selectMethod, string _nombre, string _tema, string _dificultad, int _index)
    {
        id_clase = idClase;
        curso_id = cursoId;
        fecha = _fecha;
        tiempo_maximo = tiempoMaximo;
        estado = _estado;
        codigo = _codigo;
        selectMethod = _selectMethod;
        index = _index;
        nombre = _nombre;
        tema = _tema;
        dificultad = _dificultad;

        nombreClase.text = _nombre;
        fechaClase.text = _fecha;

        if (estado == 0) 
        {
            estadoClase.text = "En espera";
            if (NetworkPlayer.localPlayer.rol.Equals("Estudiante"))
            {
                joinButton.gameObject.SetActive(false);
            }
           
        }

        else if (estado == 1)
        {
            estadoClase.text = "Por comenzar";
            if (NetworkPlayer.localPlayer.rol.Equals("Estudiante"))
            {
                joinButton.gameObject.SetActive(true);
            }
        }

        else if (estado == 2)
        {
            estadoClase.text = "Jugando";
        }
        else
        {
            Debug.Log("Error en estado");
        }
    }

    public void OnClickEdit()
    {
        StartCoroutine(Edit());
    }

    public IEnumerator Edit()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);
        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/getmaterias.php", form))
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
                materias = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);

                TeacherManager.Instance.editClass(id_clase, selectMethod, nombre, tiempo_maximo, fecha, tema, dificultad, materias.ToList());

            }
        }
    }

    public void OnClickDelete()
    {
        StartCoroutine(Delete());
    }

    public IEnumerator Delete()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_clase", id_clase);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/deleteclass.php", form))
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
                if (webRequest.downloadHandler.text.Equals("0"))
                {
                    Debug.Log("Clase borrada correctamente");
                    TeacherManager.Instance.ShowClassMenu();
                }
                
            }
        }
    }

    public void OnClickPlay()
    {
        if (NetworkPlayer.localPlayer.rol.Equals("Profesor"))
        {
            UILobby.instance.Host(id_clase, selectMethod);
            TeacherManager.Instance.ShowLobby();
        }
        else 
        {
            Debug.Log("Error OnClickPlay");
        }
        
    }

    public void OnClickJoin()
    {
        if (NetworkPlayer.localPlayer.rol.Equals("Estudiante"))
        {
            UILobby.instance.Join(id_clase);
            StudentManager.Instance.ShowLobby();
        }
        else
        {
            Debug.Log("Error OnClickJoin");
        }
    }
}
