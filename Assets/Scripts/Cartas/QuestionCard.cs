using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public struct Answer
{
    [SerializeField] private string _info;
    public string Info { get { return _info;  } }

    [SerializeField] private bool _isCorrect;
    public bool isCorrect { get { return _isCorrect; } }

}

[CreateAssetMenu(fileName = "New QuestionCard", menuName = "Quesix/new QuestionCard")]
public class QuestionCard : ScriptableObject
{
    public enum AnswerType { Multi, Single }

    [SerializeField] private string tema = string.Empty;
    public string Tema { get { return tema; } }

    [SerializeField] private Color contorno;
    public Color Contorno { get { return contorno; } }

    [SerializeField] private int id;
    public int ID { get { return id; } }

    [TextArea]
    [SerializeField] private string pregunta = string.Empty;
    public string Pregunta { get { return pregunta; } }

    [SerializeField] Answer[] _answers = null;
    public Answer[] Answers { get { return _answers; } }

    // Parametros
    [SerializeField] private bool _useTimer = false;
    public bool UseTimer { get { return _useTimer; } }

    [SerializeField] private int _timer = 0;
    public int Timer { get { return _timer; } }

    [SerializeField] private int _addCards = 1;
    public int AddCards { get { return _addCards; } }

    [SerializeField] private AnswerType _answerType = AnswerType.Multi;
    public AnswerType GetAnswerType { get { return _answerType; } }

    public List<int> GetCorrectAnswer()
    {
        List<int> CorrectAnswers = new List<int>();
        for(int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].isCorrect)
            {
                CorrectAnswers.Add(i);
            }
        }
        return CorrectAnswers;
    }
}
