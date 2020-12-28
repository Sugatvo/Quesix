using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;


public class StudentManager : MonoBehaviour
{
    [Header("Player information")]
    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI rolInfo;

    [Header("Menus")]
    [SerializeField] CanvasGroup classMenu;

    [Header("Content Area")]
    [SerializeField] float margins;
    [SerializeField] RectTransform clasesContentArea;

    [Header("Prefabs")]
    [SerializeField] ClasesData classesPrefab = null;    

    [Header("Placeholders")]
    [SerializeField] CanvasGroup emptyClases;

    [Header("Canvases")]
    [SerializeField] Canvas loginCanvas;
    [SerializeField] Canvas studentCanvas;
    [SerializeField] Canvas lobbyUI;

    private string[] clases;
    List<ClasesData> currentClases = new List<ClasesData>();


    private int clase_id = -1;
    private int curso_id = -1;

    private static StudentManager _instance;
    public static StudentManager Instance { get { return _instance; } }

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
            playerInfo.text = "BIENVENIDO - " + NetworkPlayer.localPlayer.nombre + " " + NetworkPlayer.localPlayer.apellido;
            rolInfo.text = NetworkPlayer.localPlayer.rol;
        }


        // Obtener curso_id
        StartCoroutine(GetCursoID());
    }

    public void Refresh()
    {
        StartCoroutine(CreateClasesUI(curso_id));
    }

    public IEnumerator GetCursoID()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_usuario", NetworkPlayer.localPlayer.id_user);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/student/getcurso.php", form))
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
                curso_id = int.Parse(webRequest.downloadHandler.text);
            }
        }
    }

    public void ShowClassMenu()
    {
        StartCoroutine(CreateClasesUI(curso_id));
        classMenu.alpha = 1.0f;
        classMenu.blocksRaycasts = true;
    }

    public IEnumerator CreateClasesUI(int curso_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("curso_id", curso_id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/quesix/teacher/getclases.php", form))
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
                    EraseClasesUI();
                    emptyClases.alpha = 1.0f;
                    emptyClases.blocksRaycasts = true;
                }
                else
                {
                    EraseClasesUI();
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

    void EraseClasesUI()
    {
        foreach (var user in currentClases)
        {
            Destroy(user.gameObject);
        }
        currentClases.Clear();
    }


    public void ShowLobby()
    {
        loginCanvas.gameObject.SetActive(false);
        studentCanvas.gameObject.SetActive(false);
        lobbyUI.gameObject.SetActive(true);
    }

    public void Logout()
    {
        clase_id = -1;
        classMenu.alpha = 0.0f;
        classMenu.blocksRaycasts = false;

        NetworkPlayer.localPlayer.LogOut();
        loginCanvas.gameObject.SetActive(true);
        studentCanvas.gameObject.SetActive(false);
    }

}
