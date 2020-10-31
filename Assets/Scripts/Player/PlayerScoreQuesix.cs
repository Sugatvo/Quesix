using Mirror;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PlayerScoreQuesix : NetworkBehaviour
{
    public int score;

    [SyncVar]
    public int id_team;


    [SyncVar]
    public int matchIndex;


    [SyncVar]
    public Color objectColor;

    [SyncVar]
    public Color emissionColor;

    public List<NetworkIdentity> equipo;

    [SyncVar]
    public Vector2 start_pos;

    private GUIStyle white;
    private GUIStyle grey;
    private GUIStyle black;

    // Game Manager 
    QuestionCard[] _preguntas = null;
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

        Transform ratonTransform = transform.GetChild(0).transform.Find("Raton");
        ratonTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", objectColor);
        ratonTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

        Transform orejasTransform = transform.GetChild(0).transform.Find("Cube.009");
        orejasTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", objectColor);
        orejasTransform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

        Transform narizTransform = transform.GetChild(0).transform.Find("Cube.005");
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

        var Objects=  Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "PlaceHolder");

        int i = 0;
        foreach (var item in Objects)
        {
            DropZone dropZone = item.GetComponent<DropZone>();
            dropZone.index = i;
            dropZones.Add(dropZone);
            i += 1;
        }
    }


    public void AddScore(int reward)
    {
        score += reward;
        foreach (var item in equipo)
        {
            TargetUpdateScore(item.connectionToClient, score);
        }

        // Termino de juego
        if(score == 2)
        {
            foreach (var item in equipo)
            {
                TargetFinishGame(item.connectionToClient);
            }
        }
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
        Debug.Log("FinishGame");
        string auxMinutes;
        string auxSeconds;
            
        var time = target.identity.gameObject.GetComponent<GlobalTimer>().MaxTime;

        var minutes = Mathf.Floor(time / 60);

        if(minutes < 10)
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

    void LoadQuestionCards()
    {
        Object[] objs = Resources.LoadAll("Cartas/Preguntas", typeof(QuestionCard));

        _preguntas = new QuestionCard[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            _preguntas[i] = (QuestionCard)objs[i];
        }

        _preguntas = _preguntas.Select(r => (r as QuestionCard)).Where(r => r != null).OrderBy(t => t.ID).ToArray<QuestionCard>();
    }
}
