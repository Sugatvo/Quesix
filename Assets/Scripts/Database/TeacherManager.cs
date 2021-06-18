using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class TeacherManager : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI actionMenuTitle;

    [Header("Menus")]
    [SerializeField] CanvasGroup mainMenu;
    [SerializeField] CanvasGroup cursosMenu;
    [SerializeField] CanvasGroup actionMenu;
    [SerializeField] CanvasGroup studentsInfo;
    [SerializeField] CanvasGroup classMenu;
    [SerializeField] CanvasGroup createMenu;

    [Header("Content Area")]
    [SerializeField] float margins;
    [SerializeField] RectTransform cursoContentArea;
    [SerializeField] RectTransform studentsContentArea;
    [SerializeField] RectTransform clasesContentArea;
    [SerializeField] RectTransform breadCrumbsContentArea;

    [Header("Prefabs")]
    [SerializeField] CursoData cursoPrefab = null;
    [SerializeField] StudentData studentPrefab = null;
    [SerializeField] ClasesData classesPrefab = null;
    [SerializeField] GameObject breadCrumbsPrefab = null;

    [Header("Canvases")]
    [SerializeField] Canvas loginCanvas;
    [SerializeField] Canvas teacherCanvas;
    [SerializeField] Canvas lobbyUI;

    [Header("Toggle Information")]
    [SerializeField] ToggleData materiaTogglePrefab;
    [SerializeField] GameObject mateContainer;
    [SerializeField] GameObject lenguajeContainer;

    [Header("Create class")]
    [SerializeField] TMP_InputField nameOfLessons;
    [SerializeField] ToggleGroup teamCreation;
    [SerializeField] ToggleGroup temaSelection;
    [SerializeField] ToggleGroup difficultySelection;
    [SerializeField] TMP_InputField maxTimeField;
    [SerializeField] TMP_InputField dayField;
    [SerializeField] TMP_InputField monthField;
    [SerializeField] TMP_InputField yearField;

    [Header("Create Menu")]
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] Button createClassButton;
    [SerializeField] Button editClassButton;

    [Header("Placeholders")]
    [SerializeField] CanvasGroup emptyClases;

    [Header("Settings")]
    [SerializeField] CanvasGroup settingsCanvasGroup;

    List<CursoData> currentCursos = new List<CursoData>();
    List<StudentData> currentStudents = new List<StudentData>();
    List<ToggleData> currentToggles = new List<ToggleData>();
    List<ClasesData> currentClases = new List<ClasesData>();
    public List<GameObject> currentBreadcrumbs = new List<GameObject>();

    private string[] cursos;
    private List<string> materias;
    private string[] students;
    private string[] clases;

    private int curso_id = -1;
    private int clase_id = -1;

    private static TeacherManager _instance;
    public static TeacherManager Instance { get { return _instance; } }

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
        if (NetworkPlayer.localPlayer.LoggedIn)
        {
            EraseBreadCrumbs();
            playerInfo.text = "BIENVENIDO - " + NetworkPlayer.localPlayer.nombre + " " + NetworkPlayer.localPlayer.apellido;
            GameObject initialbreadCrumbs = Instantiate(breadCrumbsPrefab, breadCrumbsContentArea);
            initialbreadCrumbs.GetComponent<TextMeshProUGUI>().text = NetworkPlayer.localPlayer.rol;
            initialbreadCrumbs.GetComponent<Button>().onClick.AddListener(() => VolverMainMenu());
            currentBreadcrumbs.Add(initialbreadCrumbs);
        }

        //Obtener cursos del profesor
        StartCoroutine(GetCursos());

        //Obtener materias
        StartCoroutine(GetToggles());
    }

    public void ShowLobby()
    {
        loginCanvas.gameObject.SetActive(false);
        teacherCanvas.gameObject.SetActive(false);
        lobbyUI.gameObject.SetActive(true);
    }

    public void Logout()
    {
        curso_id = -1;
        clase_id = -1;
        cursosMenu.alpha = 0.0f;
        actionMenu.alpha = 0.0f;
        studentsInfo.alpha = 0.0f;
        classMenu.alpha = 0.0f;
        createMenu.alpha = 0.0f;

        cursosMenu.blocksRaycasts = false;
        actionMenu.blocksRaycasts = false;
        studentsInfo.blocksRaycasts = false;
        classMenu.blocksRaycasts = false;
        createMenu.blocksRaycasts = false;
        NetworkPlayer.localPlayer.LogOut();
        loginCanvas.gameObject.SetActive(true);
        teacherCanvas.gameObject.SetActive(false);
    }

    public IEnumerator GetCursos()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_usuario", NetworkPlayer.localPlayer.id_user);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/teachercourses.php", form))
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

    public IEnumerator GetToggles()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://127.0.0.1/quesix/teacher/information.php"))
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
                materias = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
                Debug.Log(System.String.Format("There are {0} materias.", materias.Count));

                foreach (var item in materias)
                {
                    string[] aux = item.Split(new string[] { "-" }, System.StringSplitOptions.RemoveEmptyEntries);

                    if (aux[0].Equals("Matemáticas"))
                    {
                        ToggleData toggle = (ToggleData)Instantiate(materiaTogglePrefab, mateContainer.transform);
                        toggle.UpdateData(aux[1]);
                        currentToggles.Add(toggle);
                    }
                    else if (aux[0].Equals("Lenguaje"))
                    {
                        ToggleData toggle = (ToggleData)Instantiate(materiaTogglePrefab, lenguajeContainer.transform);
                        toggle.UpdateData(aux[1]);
                        currentToggles.Add(toggle);
                    }
                    else
                    {
                        Debug.Log("Error en Get Toggles");
                    }
                }
            }
        }
    }

    void EraseToggles()
    {
        foreach (var t in currentToggles)
        {
            Destroy(t.gameObject);
        }
        currentToggles.Clear();
    }

    public void ShowCursos()
    {
        CreateCursos();

        if (!currentBreadcrumbs.Find((x) => x.GetComponent<TextMeshProUGUI>().text == " / Mis Cursos / "))
        {
            GameObject breadCrumbs = Instantiate(breadCrumbsPrefab, breadCrumbsContentArea);
            breadCrumbs.GetComponent<TextMeshProUGUI>().text = " / Mis Cursos / ";
            breadCrumbs.GetComponent<Button>().onClick.AddListener(() => VolverMisCursos());
            currentBreadcrumbs.Add(breadCrumbs);
        }
        
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


    public void SelectCurso (string[] result, int cursoIndex, int id_curso, string nombre)
    {
        if (!currentBreadcrumbs.Find((x) => x.GetComponent<TextMeshProUGUI>().text == " " + nombre + " "))
        {
            GameObject breadCrumbs = Instantiate(breadCrumbsPrefab, breadCrumbsContentArea);
            breadCrumbs.GetComponent<TextMeshProUGUI>().text = " " + nombre + " ";
            breadCrumbs.GetComponent<Button>().onClick.AddListener(() => VolverCursoSeleccionado());
            currentBreadcrumbs.Add(breadCrumbs);
        }
        actionMenuTitle.text = nombre;
        students = result;
        curso_id = id_curso;
        mainMenu.alpha = 0.0f;
        mainMenu.blocksRaycasts = false;
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        actionMenu.alpha = 1.0f;
        actionMenu.blocksRaycasts = true;
    }


    void EraseBreadCrumbs()
    {
        foreach (var breadcrumb in currentBreadcrumbs)
        {
            Destroy(breadcrumb.gameObject);
        }
        currentBreadcrumbs.Clear();
    }


    public void VolverMainMenu ()
    {
        actionMenuTitle.text = "Opciones";
        for (int i = currentBreadcrumbs.Count - 1; i > -1; i--)
        {
            if (currentBreadcrumbs[i].GetComponent<TextMeshProUGUI>().text.Equals(NetworkPlayer.localPlayer.rol))
            {
                break;
            }
            else
            {
                Destroy(currentBreadcrumbs[i]);
                currentBreadcrumbs.RemoveAt(i);
            }
        }
        mainMenu.alpha = 1.0f;
        mainMenu.blocksRaycasts = true;
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
    }

    public void VolverMisCursos()
    {
        actionMenuTitle.text = "Opciones";
        for (int i = currentBreadcrumbs.Count - 1; i >-1; i--)
        {
            if (currentBreadcrumbs[i].GetComponent<TextMeshProUGUI>().text.Equals(" / Mis Cursos / "))
            {
                break;
            }
            else
            {
                Destroy(currentBreadcrumbs[i]);
                currentBreadcrumbs.RemoveAt(i);
            }
        }
        mainMenu.alpha = 1.0f;
        mainMenu.blocksRaycasts = true;
        cursosMenu.alpha = 1.0f;
        cursosMenu.blocksRaycasts = true;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
    }

    public void VolverCursoSeleccionado()
    {
        for (int i = currentBreadcrumbs.Count - 1; i > -1; i--)
        {
            if (currentBreadcrumbs[i].GetComponent<TextMeshProUGUI>().text.Equals(" " + actionMenuTitle.text + " "))
            {
                break;
            }
            else
            {
                Destroy(currentBreadcrumbs[i]);
                currentBreadcrumbs.RemoveAt(i);
            }
        }
        mainMenu.alpha = 0.0f;
        mainMenu.blocksRaycasts = false;
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = true;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        actionMenu.alpha = 1.0f;
        actionMenu.blocksRaycasts = true;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
    }

    public void ShowClassMenu()
    {
        StartCoroutine(CreateClases(curso_id));
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        classMenu.alpha = 1.0f;
        classMenu.blocksRaycasts = true;
        createMenu.alpha = 0f;
        createMenu.blocksRaycasts = false;
    }

    public IEnumerator CreateClases(int curso_id)
    {
        Debug.Log("IEnumerator CrateClases()");
        Debug.Log("curso_id = " + curso_id);
        WWWForm form = new WWWForm();
        form.AddField("curso_id", curso_id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/getclases.php", form))
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

                if (fulldata.Equals("0"))
                {
                    EraseClases();
                    emptyClases.alpha = 1.0f;
                    emptyClases.blocksRaycasts = true;
                }
                else
                {
                    EraseClases();
                    emptyClases.alpha = 0.0f;
                    emptyClases.blocksRaycasts = false;
                    clases = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);

                    float offset = -45 - margins;
                    for (int i = 0; i < clases.Length; i++)
                    {
                        string[] data = clases[i].Split('\t');

                        ClasesData clase = (ClasesData)Instantiate(classesPrefab, clasesContentArea);
                        clase.UpdateData(int.Parse(data[0]), int.Parse(data[1]), data[2], int.Parse(data[3]), int.Parse(data[4]), data[5], int.Parse(data[6]), data[7], data[8], data[9], i);
                        clase.Rect.anchoredPosition = new Vector2(0, offset);
                        offset -= (clase.Rect.sizeDelta.y + margins);
                        clasesContentArea.sizeDelta = new Vector2(clasesContentArea.sizeDelta.x, offset * -1);
                        currentClases.Add(clase);
                    }
                }
            }
        }
    }

    void EraseClases()
    {
        foreach (var user in currentClases)
        {
            Destroy(user.gameObject);
        }
        currentClases.Clear();
    }



    public void ShowStudents()
    {
        CreateStudents(students);
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;
        studentsInfo.alpha = 1.0f;
        studentsInfo.blocksRaycasts = true;
    }


    public void ShowCreateMenu()
    {
        CleanCreateClassMenu();
        // Cambiar UI
        headerText.text = "Crear clase";
        createClassButton.gameObject.SetActive(true);
        editClassButton.gameObject.SetActive(false);

        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        createMenu.alpha = 1f;
        createMenu.blocksRaycasts = true;
    }


    public void editClass(int id_clase, int selectMethod, string name, int _tiempoMax, string fecha, string tema, string dificultad, List<string> _materias)
    {
        // Cambiar UI
        headerText.text = "Editar clase";
        createClassButton.gameObject.SetActive(false);
        editClassButton.gameObject.SetActive(true);

        clase_id = id_clase;
        nameOfLessons.text = name;
        // Selected Method
        Toggle[] togglesMethod = teamCreation.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesMethod)
        {
            if (selectMethod == 0)
            {
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Al azar")){
                    t.isOn = true;
                    break;
                }
            }
            if(selectMethod == 1)
            {
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Ranking")){
                    t.isOn = true;
                    break;
                }
            }
        }

        // Select tema
        Toggle[] togglesTema = temaSelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesTema)
        {
            if (t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower().Equals(tema))
            {
                t.isOn = true;

                if (tema.Equals("matemáticas"))
                {
                    Toggle[] togglesMaterias = mateContainer.GetComponentsInChildren<Toggle>();
                    foreach (var tm in togglesMaterias)
                    {
                        if (_materias.Contains(tm.GetComponentInChildren<TextMeshProUGUI>().text))
                        {
                            tm.isOn = true;
                        }
                        else
                        {
                            tm.isOn = false;
                        }
                    }
                }
                else
                {
                    Toggle[] togglesMaterias = lenguajeContainer.GetComponentsInChildren<Toggle>();
                    foreach (var tm in togglesMaterias)
                    {
                        if (_materias.Contains(tm.GetComponentInChildren<TextMeshProUGUI>().text))
                        {
                            tm.isOn = true;
                        }
                        else
                        {
                            tm.isOn = false;
                        }
                    }
                }
                break;
            }
        }

        Toggle[] togglesDifficulty = difficultySelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesDifficulty)
        {
            if (t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower().Equals(dificultad))
            {
                t.isOn = true;
            }
        }

        maxTimeField.text = _tiempoMax.ToString();
        string[] temp = fecha.Split(new string[] { "-" }, System.StringSplitOptions.RemoveEmptyEntries);
        yearField.text = temp[0];
        monthField.text = temp[1];
        dayField.text = temp[2];

        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        actionMenu.alpha = 0.0f;
        actionMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        createMenu.alpha = 1f;
        createMenu.blocksRaycasts = true;

    }

    public void CallCreateClass()
    {
        StartCoroutine(CreateClass());
    }

    public IEnumerator CreateClass()
    {
        WWWForm form = new WWWForm();
        form.AddField("curso_id", curso_id);
        form.AddField("nombre", nameOfLessons.text);

        Toggle[] togglesMethod = teamCreation.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesMethod)
        {
            if (t.isOn)
            {
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Al azar"))
                {
                    form.AddField("selectMethod", 0);
                }
                else
                {
                    form.AddField("selectMethod", 1);
                }
                
            }
        }

        Toggle[] togglesTema = temaSelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesTema)
        {
            if (t.isOn)
            {
                form.AddField("tema", t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Matemáticas"))
                {
                    Toggle[] togglesMaterias = mateContainer.GetComponentsInChildren<Toggle>();
                    int contador = 0;
                    for (int i = 0; i < togglesMaterias.Length; i++)
                    {
                        if (togglesMaterias[i].isOn)
                        {
                            form.AddField("materia" + contador.ToString(), togglesMaterias[i].GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                            contador++;
                        }   
                    }
                    form.AddField("contador", contador.ToString());
                }
                else
                {
                    Toggle[] togglesMaterias = lenguajeContainer.GetComponentsInChildren<Toggle>();
                    int contador = 0;
                    for (int i = 0; i < togglesMaterias.Length; i++)
                    {
                        if (togglesMaterias[i].isOn)
                        {
                            form.AddField("materia" + contador.ToString(), togglesMaterias[i].GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                            contador++;
                        }
                    }
                    form.AddField("contador", contador.ToString());
                }

            }
        }

        Toggle[] togglesDifficulty = difficultySelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesDifficulty)
        {
            if (t.isOn)
            {
                form.AddField("dificultad", t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                Debug.Log("dificultad = " + t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
            }
        }

        form.AddField("tiempo_maximo", maxTimeField.text);
        form.AddField("fecha", yearField.text + "/" + monthField.text + "/" + dayField.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/createclass.php", form))
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
                    Debug.Log("Clase creada correctamente");
                    ShowClassMenu();
                }
            }
        }
    }

    public void CallUpdateClass()
    {
        StartCoroutine(UpdateClass());
    }

    public IEnumerator UpdateClass()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_clase", clase_id);
        form.AddField("nombre", nameOfLessons.text);

        Toggle[] togglesMethod = teamCreation.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesMethod)
        {
            if (t.isOn)
            {
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Al azar"))
                {
                    form.AddField("selectMethod", 0);
                }
                else
                {
                    form.AddField("selectMethod", 1);
                }

            }
        }

        Toggle[] togglesTema = temaSelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesTema)
        {
            if (t.isOn)
            {
                form.AddField("tema", t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Matemáticas"))
                {
                    Toggle[] togglesMaterias = mateContainer.GetComponentsInChildren<Toggle>();
                    int contador = 0;
                    for (int i = 0; i < togglesMaterias.Length; i++)
                    {
                        if (togglesMaterias[i].isOn)
                        {
                            form.AddField("materia" + contador.ToString(), togglesMaterias[i].GetComponentInChildren<TextMeshProUGUI>().text);
                            contador++;
                        }
                        
                    }
                    form.AddField("contador", contador.ToString());
                }
                else
                {
                    Toggle[] togglesMaterias = lenguajeContainer.GetComponentsInChildren<Toggle>();
                    int contador = 0;
                    for (int i = 0; i < togglesMaterias.Length; i++)
                    {
                        if (togglesMaterias[i].isOn)
                        {
                            form.AddField("materia" + contador.ToString(), togglesMaterias[i].GetComponentInChildren<TextMeshProUGUI>().text);
                            contador++;
                        }
                    }
                    form.AddField("contador", contador.ToString());
                }

            }
        }
        Toggle[] togglesDifficulty = difficultySelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesDifficulty)
        {
            if (t.isOn)
            {
                form.AddField("dificultad", t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
                Debug.Log("dificultad = " + t.GetComponentInChildren<TextMeshProUGUI>().text.ToLower());
            }
        }

        form.AddField("tiempo_maximo", maxTimeField.text);
        form.AddField("fecha", yearField.text + "/" + monthField.text + "/" + dayField.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://127.0.0.1/quesix/teacher/editclass.php", form))
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
                    Debug.Log("Clase editada correctamente");
                    ShowClassMenu();
                }
            }
        }
    }


    public void CleanCreateClassMenu()
    {
        nameOfLessons.text = string.Empty;
        maxTimeField.text = string.Empty;
        yearField.text = string.Empty;
        monthField.text = string.Empty;
        dayField.text = string.Empty;

        Toggle[] togglesTema = temaSelection.GetComponentsInChildren<Toggle>();
        foreach (var t in togglesTema)
        {
            if (t.isOn)
            {
                if (t.GetComponentInChildren<TextMeshProUGUI>().text.Equals("Matemáticas"))
                {
                    Toggle[] togglesMaterias = mateContainer.GetComponentsInChildren<Toggle>();
                    foreach (var tm in togglesMaterias)
                    {
                        if (tm.isOn) tm.isOn = false;
                    }
                }
                else
                {
                    Toggle[] togglesMaterias = lenguajeContainer.GetComponentsInChildren<Toggle>();
                    foreach (var tm in togglesMaterias)
                    {
                        if (tm.isOn) tm.isOn = false;
                    }
                }

            }
        }
    }


    public void HideCreateMenu()
    {
        CleanCreateClassMenu();
        cursosMenu.alpha = 0.0f;
        cursosMenu.blocksRaycasts = false;
        studentsInfo.alpha = 0.0f;
        studentsInfo.blocksRaycasts = false;
        createMenu.alpha = 0f;
        createMenu.blocksRaycasts = false;
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

    private void OnDisable()
    {
        EraseToggles();
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
