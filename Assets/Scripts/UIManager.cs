using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[Serializable()]
public struct UIManagerParameters
{
    [Header("Answer Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Resolution Screen Options")]
    [SerializeField] Color correctBGColor;
    public Color CorrectBGColor { get { return correctBGColor; } }
    [SerializeField] Color incorrectBGColor;
    public Color IncorrectBGColor { get { return incorrectBGColor; } }
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
    [SerializeField] Text resolutionStateInfoText;
    public Text ResolutionStateInfoText { get { return resolutionStateInfoText; } }

    [SerializeField] TextMeshProUGUI m_RewardText;
    public TextMeshProUGUI RewardText { get { return m_RewardText; } }

    [SerializeField] TextMeshProUGUI m_ScoreText;
    public TextMeshProUGUI ScoreText { get { return m_ScoreText; } }

    [Space]
    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }
}


public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Finish};

    [Header("References")]
    [SerializeField] GameEvents events = null;


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
    [SerializeField] CanvasGroup playerinfoCanvasGroup;
    [SerializeField] CanvasGroup handCanvasGroup;
    [SerializeField] GameObject buttonDebug;
    [SerializeField] GameObject buttonRun;


    List<AnswerData> currentAnswers = new List<AnswerData>();
    public List<AnswerData> CurrentAnswers { get { return currentAnswers; } }

    private int resStateParaHash = 0;

    private IEnumerator IE_DisplayTimedResolution = null;

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

    void DisplayResolution(ResolutionScreenType type, int count)
    {
        UpdateResUI(type, count);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGroup.blocksRaycasts = false;

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
    }

    void UpdateResUI(ResolutionScreenType type, int count)
    {
        switch(type)
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "¡Correcto!";
                uIElements.RewardText.alpha = 1f;
                uIElements.RewardText.text = $"+ {count} Digipasos";

                break;
            case ResolutionScreenType.Incorrect:
                uIElements.RewardText.alpha = 0f;
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "¡Incorrecto!";
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
        events.Ejecutar();
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
        // playerinfoCanvasGroup.alpha = 0.0f;
        // playerinfoCanvasGroup.blocksRaycasts = false;
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
        // playerinfoCanvasGroup.alpha = 0.0f;
        // playerinfoCanvasGroup.blocksRaycasts = false;
    }

    public void ShowButtons()
    {
        questionCard.alpha = 0.0f;
        questionCard.blocksRaycasts = false;
        buttonPreguntas.SetActive(true);
        buttonProgramar.SetActive(true);
        programmingCanvasGroup.alpha = 0.0f;
        programmingCanvasGroup.blocksRaycasts = false;
        buttonsCanvasGroup.alpha = 1.0f;
        buttonsCanvasGroup.blocksRaycasts = true;
        playerinfoCanvasGroup.alpha = 1.0f;
        playerinfoCanvasGroup.blocksRaycasts = true;
    }

    public void ShowCanvas()
    {
        programmingCanvasGroup.alpha = 1.0f;
        programmingCanvasGroup.blocksRaycasts = true;
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
        buttonRun.GetComponent<Button>().interactable = false;
        buttonDebug.GetComponent<Button>().interactable = true;
        handCanvasGroup.interactable = true;
        handCanvasGroup.blocksRaycasts = true;
    }

    public void OnProgrammingWithoutAuthority()
    {
        buttonDebug.GetComponent<Button>().interactable = false;
        buttonRun.GetComponent<Button>().interactable = false;
        handCanvasGroup.interactable = false;
        handCanvasGroup.blocksRaycasts = false;
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

    public void SetScoreText(string score)
    {
        uIElements.ScoreText.text = score;
    }
}
