using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New QuesitionCardTutorial", menuName = "Quesix/new QuestionCardTutorial")]
public class QuestionCardTutorial : ScriptableObject
{
    public enum AnswerType { Multi, Single }

    [SerializeField] private string tema = string.Empty;
    public string Tema
    {
        get { return tema; }
        set { tema = value; }
    }

    [SerializeField] private int index;
    public int Index
    {
        get { return index; }
        set { index = value; }
    }

    [TextArea]
    [SerializeField] private string pregunta = string.Empty;
    public string Pregunta
    {
        get { return pregunta; }
        set { pregunta = value; }
    }

    [SerializeField] Answer[] _answers = null;
    public Answer[] Answers
    {
        get { return _answers; }
        set { _answers = value; }
    }

    // Parametros
    [SerializeField] private bool _useTimer = false;
    public bool UseTimer
    {
        get { return _useTimer; }
        set { _useTimer = value; }
    }

    [SerializeField] private int _timer = 0;
    public int Timer
    {
        get { return _timer; }
        set { _timer = value; }
    }

    [SerializeField] private int _addCards = 1;
    public int AddCards { get { return _addCards; } }

    [SerializeField] private AnswerType _answerType = AnswerType.Single;
    public AnswerType GetAnswerType { get { return _answerType; } }

    public List<int> GetCorrectAnswer()
    {
        List<int> CorrectAnswers = new List<int>();
        for (int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].isCorrect)
            {
                CorrectAnswers.Add(i);
            }
        }
        return CorrectAnswers;
    }

    public string GetCorrectAnswerText()
    {
        string text = string.Empty;
        for (int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].isCorrect)
            {
                text = Answers[i].Info;
            }
        }
        return text;
    }
}