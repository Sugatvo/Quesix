using Mirror;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

public class PlayerScoreQuesix : NetworkBehaviour
{
    public int score;

    [SyncVar]
    public int id_team;

    [SyncVar]
    public int matchIndex;

    [SyncVar]
    public int clase_id;

    [SyncVar]
    public Color objectColor;

    [SyncVar]
    public Color emissionColor;

    public List<NetworkIdentity> equipo;

    private GUIStyle white;
    private GUIStyle grey;
    private GUIStyle black;

    // Game Manager 
    public QuestionCard[] _preguntas = null;
    public QuestionCard[] Preguntas { get { return _preguntas; } }

    [SerializeField] GameEvents events = null;
    [SerializeField] UIManager uiManager = null;

    [SerializeField] private List<AnswerID> pickedAnswers = new List<AnswerID>();
    public List<AnswerID> PickedAnswers
    {
        get { return pickedAnswers; }
        set { pickedAnswers = value; }
    }

    [SerializeField] private List<int> finishedQuestions = new List<int>();
    public List<int> FinishedQuestions
    {
        get { return finishedQuestions; }
        set { finishedQuestions = value; }
    }


    [SerializeField] private int currentQuestion = 0;
    public int CurrentQuestion
    {
        get { return currentQuestion; }
        set { currentQuestion = value; }
    }

    public List<GameObject> Cards;

    public List<DropZone> dropZones;

    void OnEnable()
    {
        Debug.Log("OnEnable: PlayerScoreQuesix");
        uiManager = GameObject.Find("Managers").GetComponent<UIManager>();
        LoadQuestionCards();
        white = new GUIStyle();
        white.normal.textColor = Color.white;

        grey = new GUIStyle();
        grey.normal.textColor = Color.grey;

        black = new GUIStyle();
        black.normal.textColor = Color.black;

    }

    public override void OnStartClient()
    {
        LoadQuestionCards();
        Transform ratonTransform = transform.GetChild(1).transform.Find("Raton");
        ratonTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", objectColor);
        ratonTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

        Transform orejasTransform = transform.GetChild(1).transform.Find("Cube.009");
        orejasTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", objectColor);
        orejasTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

        Transform narizTransform = transform.GetChild(1).transform.Find("Cube.005");
        narizTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", objectColor);
        narizTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

        GameObject initialLeftCard = GameObject.Find("LeftCard");
        GameObject initialFowardCard = GameObject.Find("FowardCard");
        GameObject initialBackCard = GameObject.Find("BackCard");
        GameObject initialRightCard = GameObject.Find("RightCard");

        Cards.Add(initialLeftCard);
        initialLeftCard.GetComponent<Draggable>().index = Cards.IndexOf(initialLeftCard);
        Cards.Add(initialFowardCard);
        initialFowardCard.GetComponent<Draggable>().index = Cards.IndexOf(initialFowardCard);
        Cards.Add(initialBackCard);
        initialBackCard.GetComponent<Draggable>().index = Cards.IndexOf(initialBackCard);
        Cards.Add(initialRightCard);
        initialRightCard.GetComponent<Draggable>().index = Cards.IndexOf(initialRightCard);

        var Objects= Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "PlaceHolder");
        Objects.OrderBy(x => x.name);

        int i = 0;
        foreach (var item in Objects)
        {
            DropZone dropZone = item.GetComponent<DropZone>();
            dropZone.index = i;
            dropZones.Add(dropZone);
            i += 1;
        }

        LoadQuestionCards();
    }


    public void AddScore(int reward, GameObject roboticMouse)
    {
        score += reward;
        foreach (var item in equipo)
        {
            TargetUpdateScore(item.connectionToClient, score);
        }

        // Termino de juego
        if(score == 2)
        {
            GameObject cameraPlayer = roboticMouse.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject;
            int time = cameraPlayer.GetComponent<GlobalTimer>().MaxTime - cameraPlayer.GetComponent<GlobalTimer>().CurrentTime;
            Debug.Log("AddScore -> Time " + time);

            RpcScoreUp(score, time);

            foreach (var item in equipo)
            {
                TargetFinishGame(item.connectionToClient);
            }

            NetworkServer.Destroy(roboticMouse);
        }
    }

    [ClientRpc]
    public void RpcScoreUp(int score, int time)
    {
        Debug.Log("RpcScoreUp -> Score " + score.ToString());
        Debug.Log("RpcScoreUp -> Time " + time);
        string auxMinutes;
        string auxSeconds;

        var minutes = Mathf.Floor(time / 60);

        if (minutes < 10)
        {
            auxMinutes = "0" + minutes.ToString();
        }
        else
        {
            auxMinutes = minutes.ToString();
        }
        var seconds = time - minutes * 60;

        if (seconds < 10)
        {
            auxSeconds = "0" + seconds.ToString();
        }
        else
        {
            auxSeconds = seconds.ToString();
        }

        uiManager.AddScore("Equipo " + id_team.ToString(), auxMinutes + ":" + auxSeconds);
    }


    [TargetRpc]
    public void TargetUpdateScore(NetworkConnection target, int newScore)
    {
        score = newScore;
        uiManager.SetScoreText(score.ToString());
    }


    [TargetRpc]
    public void TargetFinishGame(NetworkConnection target)
    {
        uiManager.DisplayFinish();
            
    }

    public void UpdateQuestionCardAnswer(AnswerData newAnswer)
    {
        AnswerID temp = new AnswerID
        {
            answerIndex = newAnswer.AnswerIndex,
            p1 = newAnswer.SelectionPlayer1,
            p2 = newAnswer.SelectionPlayer2
        };

        if (Preguntas[currentQuestion].GetAnswerType == QuestionCard.AnswerType.Single)
        {
            foreach (var answer in PickedAnswers.ToList())
            {
                if (answer.answerIndex != newAnswer.AnswerIndex && answer.p1 == newAnswer.SelectionPlayer1 && answer.p2 == newAnswer.SelectionPlayer2)
                {
                    uiManager.CurrentAnswers[answer.answerIndex].Reset(newAnswer.SelectionPlayer1, newAnswer.SelectionPlayer2);
                    PickedAnswers.Remove(answer);
                }
                if (answer.answerIndex == newAnswer.AnswerIndex && answer.p1 == newAnswer.SelectionPlayer1 && answer.p2 == newAnswer.SelectionPlayer2)
                {
                    PickedAnswers.Remove(temp);
                }
            }
            PickedAnswers.Add(temp);
        }

    }

    public int CompareAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            int count = 0;
            List<int> c = Preguntas[currentQuestion].GetCorrectAnswer();

            List<int> p = PickedAnswers.ToList().Select(x => x.answerIndex).ToList();
            foreach (var item in p)
            {
                if (item == c[0])
                {
                    count += 1;
                }

            }
            return count;
        }
        else
        {
            Debug.Log("No se seleccionó ninguna alternativa.");
        }
        return 0;
    }

    public string GetCorrectAnswer()
    {
        return Preguntas[currentQuestion].GetCorrectAnswerText();
    }

    void LoadQuestionCards()
    {
        StartCoroutine(CreateQuestions(clase_id));
    }


    public IEnumerator CreateQuestions(int clase_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("clase_id", clase_id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://25.90.9.119/quesix/student/getquestions.php", form))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string fulldata = webRequest.downloadHandler.text;
                string[] stringQuestion = fulldata.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);
                _preguntas = new QuestionCard[stringQuestion.Length];

                for (int i = 0; i < stringQuestion.Length-1; i++)
                {
                    string[] data = stringQuestion[i].Split('\t');

                    QuestionCard temp_question = new QuestionCard
                    {
                        Tema = data[0],
                        ID = i,
                        Pregunta = data[1],
                        UseTimer = true,
                        Timer = 120
                    };

                    WWWForm form2 = new WWWForm();
                    form2.AddField("pregunta_id", data[2]);

                    using (UnityWebRequest webRequest2 = UnityWebRequest.Post("http://25.90.9.119/quesix/student/getanswers.php", form2))
                    {
                        // Request and wait for the desired page.
                        yield return webRequest2.SendWebRequest();
                        if (webRequest2.isNetworkError)
                        {
                            Debug.Log("Error: " + webRequest2.error);
                        }
                        else
                        {
                            string fulldata2 = webRequest2.downloadHandler.text;
                            string[] stringAnswers = fulldata2.Split(new string[] { "<br>" }, System.StringSplitOptions.RemoveEmptyEntries);

                            Answer[] _answers = new Answer[4];
                            for (int j = 0; j < stringAnswers.Length; j++)
                            {
                                string[] data_answer = stringAnswers[j].Split('\t');
                                Answer temp_answer = new Answer();
                                if(int.Parse(data_answer[0]) == 0) temp_answer.isCorrect = false;
                                else temp_answer.isCorrect = true;
                                temp_answer.Info = data_answer[1];
                                _answers[j] = temp_answer;
                            }
                            temp_question.Answers = _answers;
                            _preguntas[i] = temp_question;
                        }
                    }
                }
                _preguntas = _preguntas.Select(r => (r as QuestionCard)).Where(r => r != null).OrderBy(t => t.ID).ToArray<QuestionCard>();
            }
        }
    }



}
