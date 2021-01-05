using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

[Serializable()]
public struct UIManagerParameters
{
    [Header("Answer Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Resolution Screen Options")]
    [SerializeField] Sprite correctGradient;
    public Sprite CorrectGradient { get { return correctGradient; } }

    [SerializeField] Sprite halfGradient;
    public Sprite HalfGradient { get { return halfGradient; } }

    [SerializeField] Sprite incorrectGradient;
    public Sprite IncorrectGradient { get { return incorrectGradient; } }
}

[Serializable()]
public struct UIElements
{
    [SerializeField] RectTransform answerContentArea;
    public RectTransform AnswerContentArea { get { return answerContentArea; } }
    [SerializeField] TextMeshProUGUI temaText;
    public TextMeshProUGUI TemaText { get { return temaText; } }
    [SerializeField] Image contornoImage;
    public Image ContornoImage { get { return contornoImage; } }
    [SerializeField] TextMeshProUGUI preguntaText;
    public TextMeshProUGUI PreguntaText { get { return preguntaText; } }
    [SerializeField] TextMeshProUGUI beneficioText;
    public TextMeshProUGUI BeneficioText { get { return beneficioText; } }

    [Space]
    [SerializeField] Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator { get { return resolutionScreenAnimator; } }
    [SerializeField] Image resolutionBG;
    public Image ResolutionBG { get { return resolutionBG; } }
    [SerializeField] TextMeshProUGUI resolutionStateInfoText;
    public TextMeshProUGUI ResolutionStateInfoText { get { return resolutionStateInfoText; } }

    [SerializeField] TextMeshProUGUI m_RewardText;
    public TextMeshProUGUI RewardText { get { return m_RewardText; } }

    [SerializeField] TextMeshProUGUI m_ScoreText;
    public TextMeshProUGUI ScoreText { get { return m_ScoreText; } }

    [SerializeField] CanvasGroup rewardIconCanvasGroup;
    public CanvasGroup RewardIconCanvasGroup { get { return rewardIconCanvasGroup; } }

    [SerializeField] TextMeshProUGUI rewardIconText;
    public TextMeshProUGUI RewardIconText { get { return rewardIconText; } }

    [SerializeField] CanvasGroup halfCorrectCanvasGroup;
    public CanvasGroup HalfCorrectCanvasGroup { get { return halfCorrectCanvasGroup; } }

    [SerializeField] TextMeshProUGUI correctAnswerText;
    public TextMeshProUGUI CorrectAnswerText { get { return correctAnswerText; } }

    [SerializeField] CanvasGroup incorrectCanvasGroup;
    public CanvasGroup IncorrectCanvasGroup { get { return incorrectCanvasGroup; } }

    [Space]
    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }

}


public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Half, Finish};

    [Header("References")]
    [SerializeField] GameEvents events = null;

    [Header("LeaderBoard")]
    [SerializeField] LeaderBoardInfo leaderBoardInfoPrefab = null;
    List<LeaderBoardInfo> leaderBoard = new List<LeaderBoardInfo>();
    [SerializeField] RectTransform itemContentArea;
    [SerializeField] float marginsLeaderBoard = 40f;

    [Header("UI Elements(Prefabs)")]
    [SerializeField] AnswerData answerPrefab = null;

    [SerializeField] UIElements uIElements = new UIElements();

    [Space]
    [SerializeField] UIManagerParameters parameters = new UIManagerParameters();
    [SerializeField] GameObject buttonPreguntas;
    [SerializeField] GameObject buttonProgramar;
    [SerializeField] GameObject buttonResponder;
    [SerializeField] CanvasGroup questionCard;
    [SerializeField] CanvasGroup programmingCanvasGroup;
    [SerializeField] CanvasGroup buttonsCanvasGroup;
    [SerializeField] CanvasGroup PilotoInfoCanvasGroup;
    [SerializeField] CanvasGroup CopilotoInfoCanvasGroup;
    [SerializeField] CanvasGroup handCanvasGroup;
    [SerializeField] CanvasGroup sequenceCanvasGroup;
    [SerializeField] CanvasGroup popUpRun;
    [SerializeField] CanvasGroup settingsCanvasGroup;
    [SerializeField] CanvasGroup globalTimerCanvasGroup;
    [SerializeField] CanvasGroup finishCanvasGroup;
    [SerializeField] CanvasGroup marcoQuesoCanvasGroup;
    [SerializeField] CanvasGroup marcoMovCanvasGroup;
    [SerializeField] GameObject buttonDebug;
    [SerializeField] GameObject buttonRun;
    [SerializeField] Animator finishAnimator;

    public bool isCheck = false;

    private bool isAllowTo = false;

    private bool firstProgramming = false;

    List<AnswerData> currentAnswers = new List<AnswerData>();
    public List<AnswerData> CurrentAnswers { get { return currentAnswers; } }

    private int resStateParaHash = 0;
    float offset = -25;

    private IEnumerator IE_DisplayTimedResolution = null;

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

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


    void OnEnable()
    {
        Debug.Log("OnEnable UIManager");
        events.UpdateQuestionCardUI += UpdateQuestionCardUI;
        events.DisplayResolutionScreen += DisplayResolution;
    }

    void OnDisable()
    {
        events.UpdateQuestionCardUI -= UpdateQuestionCardUI;
        events.DisplayResolutionScreen -= DisplayResolution;
    }

    void Start()
    {
        resStateParaHash = Animator.StringToHash("ScreenState");
        if (TutorialManager.Instance.isTutorial)
        {
            firstProgramming = true;
        }
        else
        {
            firstProgramming = false;
        }
    }

    void UpdateQuestionCardUI(QuestionCard card)
    {
        uIElements.TemaText.text = card.Tema;
        // uIElements.ContornoImage.color = card.Contorno;
        uIElements.PreguntaText.text = card.Pregunta;
        uIElements.BeneficioText.text = "Obtienes " + card.AddCards.ToString() + " Digipasos";
        CreateAnswers(card);
    }

    void CreateAnswers(QuestionCard card)
    {
        EraseAnswers();

        float offset = 0 - parameters.Margins;
        for (int i = 0; i < card.Answers.Length; i++)
        {
            AnswerData newAnswer = (AnswerData)Instantiate(answerPrefab, uIElements.AnswerContentArea);
            newAnswer.UpdateData(card.Answers[i].Info, i);

            newAnswer.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uIElements.AnswerContentArea.sizeDelta = new Vector2(uIElements.AnswerContentArea.sizeDelta.x, offset*-1);

            currentAnswers.Add(newAnswer);
        }
    }

    void EraseAnswers()
    {
        foreach (var answer in currentAnswers)
        {
            Destroy(answer.gameObject);
        }
        currentAnswers.Clear();
    }

    void DisplayResolution(ResolutionScreenType type, int count, string text)
    {
        UpdateResUI(type, count, text);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGroup.blocksRaycasts = false;
        uIElements.MainCanvasGroup.alpha = 0f;

        if(type != ResolutionScreenType.Finish)
        {
            if(IE_DisplayTimedResolution != null)
            {
                StopCoroutine(IE_DisplayTimedResolution);
            }
            IE_DisplayTimedResolution = DisplayTimedResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }
    }

    IEnumerator DisplayTimedResolution()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
        uIElements.MainCanvasGroup.alpha = 1f;
    }

    void UpdateResUI(ResolutionScreenType type, int count, string text)
    {
        switch(type)
        {
            case ResolutionScreenType.Correct:
                Debug.Log("Correct");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                uIElements.ResolutionBG.sprite = parameters.CorrectGradient;
                uIElements.IncorrectCanvasGroup.alpha = 0f;
                uIElements.HalfCorrectCanvasGroup.alpha = 0f;

                uIElements.ResolutionStateInfoText.text = "Respuestas \n Correctas";
                uIElements.RewardIconText.text = $"+{count}";
                uIElements.RewardText.text = $"+ {count} Digipasos";
                
                uIElements.RewardIconCanvasGroup.alpha = 1f;
                uIElements.RewardText.alpha = 1f; 
                break;

            case ResolutionScreenType.Half:
                Debug.Log("HalfCorrect");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                uIElements.ResolutionBG.sprite = parameters.HalfGradient;
                uIElements.IncorrectCanvasGroup.alpha = 0f;

                uIElements.ResolutionStateInfoText.text = "Respuestas \n Distintas";
                uIElements.RewardIconText.text = $"+{count}";
                uIElements.RewardText.text = $"+ {count} Digipasos";

                uIElements.RewardIconCanvasGroup.alpha = 1f;
                uIElements.RewardText.alpha = 1f;

                uIElements.CorrectAnswerText.text = text;
                uIElements.HalfCorrectCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -350);
                uIElements.HalfCorrectCanvasGroup.alpha = 1f;
                break;

            case ResolutionScreenType.Incorrect:
                Debug.Log("Incorrect");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                uIElements.ResolutionBG.sprite = parameters.IncorrectGradient;
                uIElements.RewardText.alpha = 0f;
                uIElements.RewardIconCanvasGroup.alpha = 0f;

                uIElements.ResolutionStateInfoText.text = "Respuestas \n Incorrectas";
                uIElements.CorrectAnswerText.text = text;
                uIElements.HalfCorrectCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-150);
                
                uIElements.HalfCorrectCanvasGroup.alpha = 1f;
                uIElements.IncorrectCanvasGroup.alpha = 1f;
                break;
        }
    }

    public void OnClickPreguntas()
    {   
        events.DisplayQuestion();
    }

    public void OnClickProgramar()
    {
        events.DisplayProgramar();
    }

    public void OnClickEjecutar()
    {
        if (!isCheck)
        {
            popUpRun.alpha = 1.0f;
            popUpRun.blocksRaycasts = true;
        }
        else
        {
            events.Ejecutar();
            isCheck = false;
        }
    }

    public void PopUpRunNo()
    {
        popUpRun.alpha = 0.0f;
        popUpRun.blocksRaycasts = false;
    }

    public void PopUpRunYes()
    {
        isCheck = true;
        OnClickEjecutar();

        popUpRun.alpha = 0.0f;
        popUpRun.blocksRaycasts = false;
    }

    public void OnClickDebug()
    {
        events.Debug();
    }

    public void ShowQuestion()
    {
        questionCard.alpha = 1.0f;
        questionCard.blocksRaycasts = true;
        buttonPreguntas.SetActive(false);
        buttonProgramar.SetActive(false);
        globalTimerCanvasGroup.alpha = 0f;
        globalTimerCanvasGroup.blocksRaycasts = false;
    }


    public void DisableQuestion()
    {
        foreach (var item in currentAnswers)
        {
            item.GetComponent<Button>().interactable = false;
        }

        if(buttonResponder.GetComponent<Button>().interactable == true){
            isAllowTo = true;
        }
        buttonResponder.GetComponent<Button>().interactable = false;
    }

    public void EnableQuestion()
    {
        foreach (var item in currentAnswers)
        {
            item.GetComponent<Button>().interactable = true;
        }

        if (isAllowTo)
        {
            buttonResponder.GetComponent<Button>().interactable = true;
            isAllowTo = false;
        }
    }

    public void HideAll()
    {
        questionCard.alpha = 0.0f;
        questionCard.blocksRaycasts = false;
        buttonPreguntas.SetActive(false);
        buttonProgramar.SetActive(false);
        programmingCanvasGroup.alpha = 0.0f;
        programmingCanvasGroup.blocksRaycasts = false;
        buttonsCanvasGroup.alpha = 0.0f;
        buttonsCanvasGroup.blocksRaycasts = false;
    }

    public void ShowInterface()
    {
        questionCard.alpha = 0.0f;
        questionCard.blocksRaycasts = false;
        buttonPreguntas.SetActive(true);
        buttonProgramar.SetActive(true);
        programmingCanvasGroup.alpha = 0.0f;
        programmingCanvasGroup.blocksRaycasts = false;
        buttonsCanvasGroup.alpha = 1.0f;
        buttonsCanvasGroup.blocksRaycasts = true;

        PilotoInfoCanvasGroup.alpha = 1.0f;
        PilotoInfoCanvasGroup.blocksRaycasts = true;

        CopilotoInfoCanvasGroup.alpha = 1.0f;
        CopilotoInfoCanvasGroup.blocksRaycasts = true;

        globalTimerCanvasGroup.alpha = 1f;
        globalTimerCanvasGroup.blocksRaycasts = true;
    }

    public void ShowCanvas()
    {
        programmingCanvasGroup.alpha = 1.0f;
        programmingCanvasGroup.blocksRaycasts = true;

        if (firstProgramming)
        {
            transform.GetComponent<TutorialManager>().p_Animator.SetBool("FirstProgramming", firstProgramming);
            firstProgramming = false;
        }
        buttonsCanvasGroup.alpha = 0.0f;
        buttonsCanvasGroup.blocksRaycasts = false;
    }

    public void DisableButtons()
    {
        buttonPreguntas.GetComponent<Button>().interactable = false;
        buttonProgramar.GetComponent<Button>().interactable = false;
        buttonResponder.GetComponent<Button>().interactable = false;
    }

    public void EnableButtons()
    {
        buttonPreguntas.GetComponent<Button>().interactable = true;
        buttonProgramar.GetComponent<Button>().interactable = false;
        buttonResponder.GetComponent<Button>().interactable = true;
    }

    public void EnableProgramar()
    {
        buttonProgramar.GetComponent<Button>().interactable = true;
    }

    public void OnProgrammingWithAuthority()
    {
        buttonRun.GetComponent<Button>().interactable = true;
        buttonDebug.GetComponent<Button>().interactable = true;
        handCanvasGroup.interactable = true;
        handCanvasGroup.blocksRaycasts = true;
        sequenceCanvasGroup.blocksRaycasts = true;
    }

    public void OnProgrammingWithoutAuthority()
    {
        buttonDebug.GetComponent<Button>().interactable = false;
        buttonRun.GetComponent<Button>().interactable = false;
        handCanvasGroup.interactable = false;
        handCanvasGroup.blocksRaycasts = false;
        sequenceCanvasGroup.blocksRaycasts = false;
    }

    public void DisabledButtonDebug()
    {
        buttonDebug.GetComponent<Button>().interactable = false;
    }

    public void EnabledButtonDebug()
    {
        buttonDebug.GetComponent<Button>().interactable = true;
    }


    public void DisabledButtonRun()
    {
        buttonRun.GetComponent<Button>().interactable = false;
    }

    public void EnabledButtonRun()
    {
        buttonRun.GetComponent<Button>().interactable = true;
    }

    public void DisabledMovement()
    {
        handCanvasGroup.blocksRaycasts = false;
        sequenceCanvasGroup.blocksRaycasts = false;
    }

    public void EnabledMovement()
    {
        handCanvasGroup.blocksRaycasts = true;
        sequenceCanvasGroup.blocksRaycasts = true;
    }

    public void EnabledChangeColor()
    {
        sequenceCanvasGroup.blocksRaycasts = true;
    }
    public void SetScoreText(string score)
    {
        uIElements.ScoreText.text = score;
    }

    public void SetRol(bool pilot, bool copilot)
    {
        transform.GetComponent<TutorialManager>().m_Animator.SetBool("isPilot", pilot);
        transform.GetComponent<TutorialManager>().m_Animator.SetBool("isCopilot", copilot);
    }

    public void SetRolProgramming(bool pilot, bool copilot)
    {
        transform.GetComponent<TutorialManager>().p_Animator.SetBool("isPilot", pilot);
        transform.GetComponent<TutorialManager>().p_Animator.SetBool("isCopilot", copilot);
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

    public void AddScore(string teamName, string timeScore)
    {
        offset -= marginsLeaderBoard;
        LeaderBoardInfo newLeaderBoardItem = (LeaderBoardInfo)Instantiate(leaderBoardInfoPrefab, itemContentArea);
        newLeaderBoardItem.SetAttributes(teamName, timeScore);
        newLeaderBoardItem.Rect.anchoredPosition = new Vector2(0, offset);
        offset -= (newLeaderBoardItem.Rect.sizeDelta.y);
        itemContentArea.sizeDelta = new Vector2(itemContentArea.sizeDelta.x, offset * -1);
        leaderBoard.Add(newLeaderBoardItem);
    }


    public void DisplayFinish()
    {
        uIElements.MainCanvasGroup.alpha = 0f;
        uIElements.MainCanvasGroup.blocksRaycasts = false;

        finishCanvasGroup.alpha = 1f;
        finishCanvasGroup.blocksRaycasts = true;
        finishAnimator.SetBool("isFinished", true);
    }

    public void HideFinish()
    {
        finishCanvasGroup.alpha = 0f;
        finishCanvasGroup.blocksRaycasts = false;
    }

    public void GoBackToLobby()
    {
        events.GoBackToLobby();
    }
    
    public void HideForTeacher()
    {
        Debug.Log("HideForTeacher()");
        TutorialManager.Instance.m_Animator.enabled = false;
        TutorialManager.Instance.p_Animator.enabled = false;
        buttonsCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.blocksRaycasts = false;

        PilotoInfoCanvasGroup.alpha = 0f;
        PilotoInfoCanvasGroup.blocksRaycasts = false;

        CopilotoInfoCanvasGroup.alpha = 0f;
        CopilotoInfoCanvasGroup.blocksRaycasts = false;

        marcoQuesoCanvasGroup.alpha = 0f;
        marcoQuesoCanvasGroup.blocksRaycasts = false;

        marcoMovCanvasGroup.alpha = 0f;
        marcoMovCanvasGroup.blocksRaycasts = false;

    }
}
