using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameEvents", menuName = "Quesix/new GameEvents")]
public class GameEvents : ScriptableObject
{
    public delegate void UpdateQuestionCardUICallback(QuestionCard questionCard);
    public UpdateQuestionCardUICallback UpdateQuestionCardUI;

    public delegate void UpdateQuestionCardAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionCardAnswerCallback UpdateQuestionCardAnswer;

    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, int count);
    public DisplayResolutionScreenCallback DisplayResolutionScreen;

    public delegate void DisplayQuestionCallback();
    public DisplayQuestionCallback DisplayQuestion;

    public delegate void DisplayProgramarCallback();
    public DisplayProgramarCallback DisplayProgramar;

    public delegate void SelectAnswerCallback(int AnswerIndex);
    public SelectAnswerCallback SelectAnswer;

    public delegate void AcceptAnswerCallback();
    public AcceptAnswerCallback AcceptAnswer;


    public delegate void ShowQuestionCallback(int rand);
    public ShowQuestionCallback ShowQuestion;

    public delegate void EjecutarCallback();
    public EjecutarCallback Ejecutar;

    public delegate void DebugCallback();
    public DebugCallback Debug;

    public delegate void RefreshCallback();
    public RefreshCallback RefreshUsers;

    public delegate void RefreshReassignCallback();
    public RefreshReassignCallback RefreshReassign;

    public delegate void SelectCursoCallback(string[] students, int id_curso);
    public SelectCursoCallback SelectCurso;
}
