using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Quesix/new GameEvents")]
public class GameEvents : ScriptableObject
{
    public delegate void UpdateQuestionCardUICallback(QuestionCard questionCard);
    public UpdateQuestionCardUICallback UpdateQuestionCardUI;

    public delegate void UpdateQuestionCardAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionCardAnswerCallback UpdateQuestionCardAnswer;

    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, int count, string text);
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

    public delegate void AssignCardCallback(int index);
    public AssignCardCallback AssignCard;

    public delegate void ReassignCardCallback(int index);
    public ReassignCardCallback ReassignCard;

    public delegate void CreateCardInstanceCallback(int indexPrefab);
    public CreateCardInstanceCallback CreateCardInstance;

    public delegate void SynchronizeOnBeginDragCallback(int index);
    public SynchronizeOnBeginDragCallback SynchronizeOnBeginDrag;

    public delegate void SynchronizeOnDragCallback(int index, Vector2 position, float oldX, float oldY);
    public SynchronizeOnDragCallback SynchronizeOnDrag;

    public delegate void SynchronizeOnDropCallback(int cardIndex, int dropIndex);
    public SynchronizeOnDropCallback SynchronizeOnDrop;

    public delegate void SynchronizeOnEndDragCallback(int index);
    public SynchronizeOnEndDragCallback SynchronizeOnEndDrag;

    public delegate void PlayerIsMovingCallback(bool statement);
    public PlayerIsMovingCallback PlayerIsMoving;

    public delegate void GoBackToLobbyCallback();
    public GoBackToLobbyCallback GoBackToLobby;
}
