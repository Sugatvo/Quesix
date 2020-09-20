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


    [SyncVar] private Color objectColor;
    public Color ObjectColor
    {
        get { return objectColor; }
        set { objectColor = value; }
    }

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
        Transform cubeTransform = transform.GetChild(0).transform.Find("Cube");
        cubeTransform.GetComponent<MeshRenderer>().material.color = ObjectColor;
    }


    public void AddScore(int reward)
    {
        score += reward;
        foreach (var item in equipo)
        {
            TargetUpdateScore(item.connectionToClient, score);
        }
    }

    [TargetRpc]
    public void TargetUpdateScore(NetworkConnection target, int newScore)
    {
        score = newScore;
        uiManager.SetScoreText(score.ToString());
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
