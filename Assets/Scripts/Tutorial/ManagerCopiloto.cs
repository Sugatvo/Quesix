using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerCopiloto : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Half, Finish };

    [Header("Tutorial")]
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup settingsCanvasGroup;
    [SerializeField] CanvasGroup popUpExitTutorial;
    [SerializeField] CanvasGroup waitingCopilotoDecision;
    [SerializeField] Animator tutorialAnimator = null;
    [SerializeField] Animator programmingAnimator = null;
    [SerializeField] GameObject teamObject;
    [SerializeField] Image checkBoxFirstObjective;
    [SerializeField] Image checkBoxSecondObjective;
    [SerializeField] Sprite successfulObjective;
    [SerializeField] CanvasGroup terminoTutorial;
    private IEnumerator IE_DelayAccept = null;
    private IEnumerator IE_DisplayPeriodically = null;
    private IEnumerator IE_DelayedProgrammingPhase = null;
    public bool isFirstQuestion;
    public bool isFirstProgramming;
    public bool isSelectedByTeammate;
    public bool isRespondingQuestions;
    public bool isDisplaying;
    public bool isReadyToAccept;
    public bool isFirstReview;

    [Header("Main UI")]
    [SerializeField] CanvasGroup globalTimerCanvasGroup;
    [SerializeField] CanvasGroup programmingCanvasGroup;
    [SerializeField] CanvasGroup buttonsCanvasGroup;
    [SerializeField] CanvasGroup teamInfoCanvasGroup;
    [SerializeField] CanvasGroup changeRolesCanvasGroup;
    [SerializeField] GameObject buttonPreguntas;
    [SerializeField] GameObject buttonProgramar;
    [SerializeField] GameObject buttonResponder;
    [SerializeField] TextMeshProUGUI NombrePiText;
    [SerializeField] TextMeshProUGUI ApellidoPiText;
    [SerializeField] TextMeshProUGUI NombreCoText;
    [SerializeField] TextMeshProUGUI ApellidoCoText;
    [SerializeField] TextMeshProUGUI ScoreText;
    [SerializeField] Animator teamInfoAnimator = null;

    public int score;

    [Header("Programming UI")]
    [SerializeField] GameObject buttonDebug;
    [SerializeField] GameObject buttonRun;
    [SerializeField] CanvasGroup handCanvasGroup;
    [SerializeField] CanvasGroup sequenceCanvasGroup;
    [SerializeField] CanvasGroup popUpRun;
    [SerializeField] RectTransform sequence;
    [SerializeField] Animator timerAnimator_Programming = null;
    [SerializeField] TextMeshProUGUI timerText_Programming = null;
    [SerializeField] TextMeshProUGUI NombrePiProText;
    [SerializeField] TextMeshProUGUI ApellidoPiProText;
    [SerializeField] TextMeshProUGUI NombreCoProText;
    [SerializeField] TextMeshProUGUI ApellidoCoProText;
    private IEnumerator IE_StartTimerProgramming = null;
    public bool isCheck = false;


    [Header("ResolutionScreen")]
    [SerializeField] Animator resolutionScreenAnimator = null;
    [SerializeField] Image resolutionBG;
    [SerializeField] TextMeshProUGUI resolutionStateInfoText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] CanvasGroup rewardIconCanvasGroup;
    [SerializeField] TextMeshProUGUI rewardIconText;
    [SerializeField] CanvasGroup halfCorrectCanvasGroup;
    [SerializeField] TextMeshProUGUI correctAnswerResolutionText;
    [SerializeField] CanvasGroup incorrectCanvasGroup;
    private int resStateParaHash = 0;
    private IEnumerator IE_DisplayTimedResolution = null;

    [Header("Resolution Screen Options")]
    [SerializeField] Sprite correctGradient;
    [SerializeField] Sprite halfGradient;
    [SerializeField] Sprite incorrectGradient;

    [Header("Arrays")]
    public QuestionCardTutorial[] preguntas = null;
    public int currentQuestion = 0;
    public List<AnswerDataTutorial> currentAnswers = new List<AnswerDataTutorial>();
    public List<AnswerID> pickedAnswers = new List<AnswerID>();
    public List<int> finishedQuestions = new List<int>();

    [Header("QuestionCard")]
    [SerializeField] CanvasGroup questionCard;
    [SerializeField] RectTransform answerContentArea;
    [SerializeField] AnswerDataTutorial answerTutorialPrefab = null;
    [SerializeField] TextMeshProUGUI temaText;
    [SerializeField] TextMeshProUGUI preguntaText;
    [SerializeField] TextMeshProUGUI beneficioText;

    [Header("QuestionCardTimer")]
    [SerializeField] Animator timerAnimator;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor = Color.white;
    private int timerStateParaHash = 0;
    private IEnumerator IE_StartTimer = null;
    private IEnumerator IE_WaitTillNextRound = null;

    [Header("Movement Cards")]
    [SerializeField] public TextMeshProUGUI cardCountText;
    [SerializeField] public TextMeshProUGUI leftCountText;
    [SerializeField] public TextMeshProUGUI fowardCountText;
    [SerializeField] public TextMeshProUGUI backCountText;
    [SerializeField] public TextMeshProUGUI rightCountText;
    public List<GameObject> Cards;
    public GameObject currentCardInMovement;

    [Header("Cards prefab")]
    [SerializeField] public GameObject leftPrefab;
    [SerializeField] public GameObject fowardPrefab;
    [SerializeField] public GameObject backPrefab;
    [SerializeField] public GameObject rightPrefab;

    [Header("Cards spawners")]
    [SerializeField] public Transform leftSpawner;
    [SerializeField] public Transform fowardSpawner;
    [SerializeField] public Transform backSpawner;
    [SerializeField] public Transform rightSpawner;

    [Header("Dropzones")]
    public List<DropZoneTutorial> dropZones = new List<DropZoneTutorial>();

    public int[] tutorialCards;
    public int currentTutorialCard;

    private static ManagerCopiloto _instance;
    public static ManagerCopiloto Instance { get { return _instance; } }

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

    private void Start()
    {
        tutorialCards = new int[] { 1, 1, 0, 0, 2, 2 };
        currentTutorialCard = 0;
        LoadQuestionCards();
        score = 0;
        isFirstQuestion = true;
        isRespondingQuestions = true;
        isDisplaying = false;
        isReadyToAccept = true;
        isFirstProgramming = true;
        isFirstReview = true;
        resStateParaHash = Animator.StringToHash("ScreenState");
        timerStateParaHash = Animator.StringToHash("TimeState");
        timerDefaultColor = timerText.color;

        List<DropZoneTutorial> dropZone = FindObjectsOfType<DropZoneTutorial>().OrderBy(x => x.gameObject.name).ToList();

        int i = 0;
        Debug.Log("Dropzone order");
        foreach (DropZoneTutorial dz in dropZone)
        {
            Debug.Log("dropzone name = " + dz.gameObject.name);
            dz.index = i;
            dropZones.Add(dz);
            i += 1;
        }
    }

    void LoadQuestionCards()
    {
        Object[] objs = Resources.LoadAll("Cartas/QuestionCardTutorial", typeof(QuestionCardTutorial));

        preguntas = new QuestionCardTutorial[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            preguntas[i] = (QuestionCardTutorial)objs[i];
        }

        preguntas = preguntas.Select(r => (r as QuestionCardTutorial)).Where(r => r != null).OrderBy(t => t.Index).ToArray<QuestionCardTutorial>();
    }

    // Fase de Preguntas
    public void Display()
    {
        isDisplaying = true;
        isSelectedByTeammate = false;
        waitingCopilotoDecision.alpha = 0f;
        EraseAnswers();
        QuestionCardTutorial pregunta = GetRandomQuestion();
        UpdateQuestionCardUI(pregunta);
        UptadeTimer(pregunta.UseTimer);

        questionCard.alpha = 1.0f;
        questionCard.blocksRaycasts = true;
        buttonsCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.blocksRaycasts = false;
        globalTimerCanvasGroup.alpha = 0f;
        globalTimerCanvasGroup.blocksRaycasts = false;

        if (isFirstQuestion)
        {
            tutorialAnimator.SetBool("FaseDePreguntas", true);
            questionCard.blocksRaycasts = false;
            DisableButtons();
        }

    }

    int GetRandomQuestionIndex()
    {
        int random = 0;
        if (finishedQuestions.Count < preguntas.Length)
        {
            do
            {
                random = Random.Range(0, preguntas.Length);
            } while (finishedQuestions.Contains(random) || random == currentQuestion);
        }
        return random;
    }

    QuestionCardTutorial GetRandomQuestion()
    {
        int randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;
        return preguntas[currentQuestion];
    }


    void UpdateQuestionCardUI(QuestionCardTutorial card)
    {
        temaText.text = card.Tema;
        preguntaText.text = card.Pregunta;
        beneficioText.text = "Obtienes " + card.AddCards.ToString() + " Digipasos";
        CreateAnswers(card);
    }

    void CreateAnswers(QuestionCardTutorial card)
    {
        EraseAnswers();

        float offset = 0.05f;
        float sizeY = 0.2f;
        for (int i = 0; i < card.Answers.Length; i++)
        {
            AnswerDataTutorial newAnswer = (AnswerDataTutorial)Instantiate(answerTutorialPrefab, answerContentArea);
            newAnswer.UpdateData(card.Answers[i].Info, i);
            newAnswer.Rect.anchorMin = new Vector2(0f, sizeY * i + offset * i);
            newAnswer.Rect.anchorMax = new Vector2(1f, sizeY * (i + 1) + offset * i);
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

    public void TargetSelection(int AnswerIndex, int player)
    {
        // Owner
        if (player == 1)
        {
            currentAnswers[AnswerIndex].SelectionPlayer1 = true;
            currentAnswers[AnswerIndex].SelectionPlayer2 = false;
        }
        // Teammate
        if (player == 2)
        {
            currentAnswers[AnswerIndex].SelectionPlayer1 = false;
            currentAnswers[AnswerIndex].SelectionPlayer2 = true;
        }
        currentAnswers[AnswerIndex].UpdateAnswersUI();
        UpdateQuestionCardAnswer(currentAnswers[AnswerIndex]);
    }

    public void UpdateQuestionCardAnswer(AnswerDataTutorial newAnswer)
    {
        AnswerID temp = new AnswerID
        {
            answerIndex = newAnswer.AnswerIndex,
            p1 = newAnswer.SelectionPlayer1,
            p2 = newAnswer.SelectionPlayer2
        };

        foreach (var answer in pickedAnswers.ToList())
        {
            if (answer.answerIndex != newAnswer.AnswerIndex && answer.p1 == newAnswer.SelectionPlayer1 && answer.p2 == newAnswer.SelectionPlayer2)
            {
                currentAnswers[answer.answerIndex].Reset(newAnswer.SelectionPlayer1, newAnswer.SelectionPlayer2);
                pickedAnswers.Remove(answer);
            }
            if (answer.answerIndex == newAnswer.AnswerIndex && answer.p1 == newAnswer.SelectionPlayer1 && answer.p2 == newAnswer.SelectionPlayer2)
            {
                pickedAnswers.Remove(temp);
            }
        }
        pickedAnswers.Add(temp);
    }

    public int CompareAnswers()
    {
        if (pickedAnswers.Count > 0)
        {
            int count = 0;
            List<int> c = preguntas[currentQuestion].GetCorrectAnswer();
            List<int> p = pickedAnswers.ToList().Select(x => x.answerIndex).ToList();
            foreach (var item in p)
            {
                if (item == c[0])
                {
                    count += 1;
                }
            }
            ErasePickedAnswers();
            return count;
        }
        else
        {
            Debug.Log("No se seleccionó ninguna alternativa.");
        }
        return 0;
    }


    public void ErasePickedAnswers()
    {
        foreach (var answer in pickedAnswers.ToList())
        {
            Debug.Log(answer);
            Instance.pickedAnswers.Remove(answer);
        }
        Instance.pickedAnswers.Clear();
    }
    public string GetCorrectAnswer()
    {
        return preguntas[currentQuestion].GetCorrectAnswerText();
    }

    public void Accept()
    {
        Debug.Log("Accept()");
        UptadeTimer(false);

        int CountCorrectAnswers = CompareAnswers();
        string correctAnswerText = GetCorrectAnswer();
        finishedQuestions.Add(currentQuestion);

        if (CountCorrectAnswers > 0)
        {
            for (int i = 0; i < CountCorrectAnswers; i++)
            {
                int index = tutorialCards[currentTutorialCard];
                Debug.Log("index = " + index);
                AddCard(index);
                currentTutorialCard += 1;
            }
        }

        ManagerCopiloto.ResolutionScreenType type;

        if (CountCorrectAnswers == 2)
        {
            type = ManagerCopiloto.ResolutionScreenType.Correct;
        }
        else if (CountCorrectAnswers == 1)
        {
            type = ManagerCopiloto.ResolutionScreenType.Half;
        }
        else
        {
            type = ManagerCopiloto.ResolutionScreenType.Incorrect;
        }

        DisplayResolutionScreen(type, CountCorrectAnswers, correctAnswerText);

        if (type != ManagerCopiloto.ResolutionScreenType.Finish)
        {
            if (IE_WaitTillNextRound != null)
            {
                StopCoroutine(IE_WaitTillNextRound);
            }
            IE_WaitTillNextRound = WaitTillNextRound();
            StartCoroutine(IE_WaitTillNextRound);
        }
    }

    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        // Mostrar ui denuevo
        ShowInterface();
    }

    void DisplayResolutionScreen(ResolutionScreenType type, int count, string text)
    {
        UpdateResUI(type, count, text);
        resolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        mainCanvasGroup.blocksRaycasts = false;
        mainCanvasGroup.alpha = 0f;

        if (type != ResolutionScreenType.Finish)
        {
            if (IE_DisplayTimedResolution != null)
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
        resolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        mainCanvasGroup.blocksRaycasts = true;
        mainCanvasGroup.alpha = 1f;
        isDisplaying = false;
        isReadyToAccept = true;
        waitingCopilotoDecision.alpha = 1f;
    }

    void UpdateResUI(ResolutionScreenType type, int count, string text)
    {
        switch (type)
        {
            case ResolutionScreenType.Correct:
                Debug.Log("Correct");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                resolutionBG.sprite = correctGradient;
                incorrectCanvasGroup.alpha = 0f;
                halfCorrectCanvasGroup.alpha = 0f;

                resolutionStateInfoText.text = "Respuestas \n Correctas";
                rewardIconText.text = $"+{count}";
                rewardText.text = $"+ {count} Digipasos";

                rewardIconCanvasGroup.alpha = 1f;
                rewardText.alpha = 1f;
                break;

            case ResolutionScreenType.Half:
                Debug.Log("HalfCorrect");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                resolutionBG.sprite = halfGradient;
                incorrectCanvasGroup.alpha = 0f;

                resolutionStateInfoText.text = "Respuestas \n Distintas";
                rewardIconText.text = $"+{count}";
                rewardText.text = $"+ {count} Digipasos";

                rewardIconCanvasGroup.alpha = 1f;
                rewardText.alpha = 1f;

                correctAnswerResolutionText.text = text;
                halfCorrectCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -350);
                halfCorrectCanvasGroup.alpha = 1f;
                break;

            case ResolutionScreenType.Incorrect:
                Debug.Log("Incorrect");
                Debug.Log("type: " + type);
                Debug.Log("count: " + count);
                Debug.Log("text: " + text);
                resolutionBG.sprite = incorrectGradient;
                rewardText.alpha = 0f;
                rewardIconCanvasGroup.alpha = 0f;

                resolutionStateInfoText.text = "Respuestas \n Incorrectas";
                correctAnswerResolutionText.text = text;
                halfCorrectCanvasGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);

                halfCorrectCanvasGroup.alpha = 1f;
                incorrectCanvasGroup.alpha = 1f;
                break;
        }
    }


    public void AddCard(int randomIndex)
    {
        if (randomIndex == 0)
        {
            leftCountText.text = (int.Parse(leftCountText.text) + 1).ToString();
        }
        else if (randomIndex == 1)
        {
            fowardCountText.text = (int.Parse(fowardCountText.text) + 1).ToString();
        }
        else if (randomIndex == 2)
        {
            backCountText.text = (int.Parse(backCountText.text) + 1).ToString();
        }
        else
        {
            rightCountText.text = (int.Parse(rightCountText.text) + 1).ToString();
        }

        cardCountText.text = (int.Parse(cardCountText.text) + 1).ToString();

        if (int.Parse(cardCountText.text) >= 5)
        {
            EnableProgramar();
            FirstObjectiveSuccessful();
            if (IE_DisplayPeriodically != null)
            {
                StopCoroutine(IE_DisplayPeriodically);
            }
            IE_DelayedProgrammingPhase = DelayedProgrammingPhase();
            StartCoroutine(IE_DelayedProgrammingPhase);
        }
    }

    IEnumerator DelayedProgrammingPhase()
    {
        yield return new WaitForSeconds(4f);
        Programar();
    }

    void UptadeTimer(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);

                timerAnimator.SetInteger(timerStateParaHash, 2);
                break;
            case false:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }

                timerAnimator.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        while (isFirstQuestion)
        {
            yield return new WaitForSeconds(1.0f);
        }

        var totalTime = 120;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while (timeLeft > 0)
        {
            if (pickedAnswers.Count == 2 && isReadyToAccept)
            {
                isReadyToAccept = false;
                if (IE_DelayAccept != null)
                {
                    StopCoroutine(IE_DelayAccept);
                }
                IE_DelayAccept = DelayAccept();
                StartCoroutine(IE_DelayAccept);
            }
            else if (pickedAnswers.Count == 1 && !isSelectedByTeammate)
            {
                RandomOrCorrectAnswerSelection();
            }
            else
            {
                DisableResponder();
            }

            timeLeft--;
            if (timeLeft < totalTime / 2 && timeLeft > totalTime / 4)
            {
                timerText.color = timerHalfWayOutColor;
            }
            if (timeLeft < totalTime / 4)
            {
                timerText.color = timerAlmostOutColor;
            }
            timerText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator DelayAccept()
    {
        yield return new WaitForSeconds(2f);
        Accept();
    }

    public void RandomOrCorrectAnswerSelection()
    {
        int rand_or_correct = Random.Range(0, 3);
        int index;
        if (rand_or_correct == 0)
        {
            index = preguntas[currentQuestion].GetCorrectAnswer()[0];
        }
        else
        {
            index = Random.Range(0, 4);
        }
        TargetSelection(index, 2);
        isSelectedByTeammate = true;

    }


    // Fase de Programación
    public void Programar()
    {
        ShowProgrammingCanvas();
        OnProgrammingWithoutAuthority();
        UptadeTimerProgramming(true);

    }

    public void ShowProgrammingCanvas()
    {
        programmingCanvasGroup.alpha = 1.0f;
        programmingCanvasGroup.blocksRaycasts = true;
        mainCanvasGroup.alpha = 0.0f;
        mainCanvasGroup.blocksRaycasts = false;
        buttonsCanvasGroup.alpha = 0.0f;
        buttonsCanvasGroup.blocksRaycasts = false;
        programmingAnimator.SetBool("StartProgrammingTutorial", true);
        TutorialSequenceSimulator.Instance.SimulateSequence();
    }

    public void OnProgrammingWithoutAuthority()
    {
        buttonRun.GetComponent<Button>().interactable = false;
        buttonDebug.GetComponent<Button>().interactable = false;
        handCanvasGroup.interactable = false;
        handCanvasGroup.blocksRaycasts = false;
        sequenceCanvasGroup.blocksRaycasts = false;
        sequenceCanvasGroup.interactable = false;
    }

    void UptadeTimerProgramming(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimerProgramming = StartTimerProgramming();
                StartCoroutine(IE_StartTimerProgramming);

                if (!isFirstProgramming)
                {
                    timerAnimator_Programming.SetInteger(timerStateParaHash, 2);
                }

                break;
            case false:
                if (IE_StartTimerProgramming != null)
                {
                    StopCoroutine(IE_StartTimerProgramming);
                }

                timerAnimator_Programming.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimerProgramming()
    {
        while (isFirstProgramming)
        {
            yield return new WaitForSeconds(1.0f);
        }

        timerAnimator_Programming.SetInteger(timerStateParaHash, 2);
        var totalTime = 120;
        var timeLeft = totalTime;

        timerText_Programming.color = timerDefaultColor;
        while (timeLeft > 0)
        {
            timeLeft--;
            if (timeLeft < totalTime / 2 && timeLeft > totalTime / 4)
            {
                timerText_Programming.color = timerHalfWayOutColor;
            }
            if (timeLeft < totalTime / 4)
            {
                timerText_Programming.color = timerAlmostOutColor;
            }
            timerText_Programming.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }
        TimeOutProgramming();
    }

    public void TimeOutProgramming()
    {
        isCheck = false;
        Ejecutar();
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
            Ejecutar();
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

    public void Ejecutar()
    {
        UptadeTimerProgramming(false);
        StartCoroutine(HideAndShow());
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(1.0f);
        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (placeHolder.childCount > 0)
            {
                MovCardData card = placeHolder.GetChild(0).GetComponent<MovCardData>();

                if (!teamObject.GetComponent<PlayerController3DTutorial>().restartPosition)
                {
                    Move(card.CardAction);
                    yield return new WaitForSeconds(1.2f);
                }
                else
                {
                    break;
                }
            }
        }
        while (teamObject.GetComponent<PlayerController3DTutorial>().restartPosition)
        {
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        BorrarCartas();
        StartCoroutine(CambiarAutoridadPiloto());
        SecondObjectiveSuccessful();
    }

    public void Move(string _movimiento)
    {
        if (_movimiento.Equals("Avanzar"))
        {
            teamObject.GetComponent<PlayerController3DTutorial>().Avanzar();
        }
        else if (_movimiento.Equals("Retroceder"))
        {
            teamObject.GetComponent<PlayerController3DTutorial>().Retroceder();
        }
        else if (_movimiento.Equals("Izquierda"))
        {
            teamObject.GetComponent<PlayerController3DTutorial>().GirarIzquierda();
        }
        else
        {
            teamObject.GetComponent<PlayerController3DTutorial>().GirarDerecha();
        }
    }

    public void BorrarCartas()
    {
        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (placeHolder.childCount > 0)
            {
                GameObject movementGameObject = placeHolder.GetChild(0).gameObject;
                DropZoneTutorial dropZone = placeHolder.GetComponent<DropZoneTutorial>();
                if (dropZone != null)
                {
                    dropZone.movementAction.text = string.Empty;
                }
                else
                {
                    Debug.Log("DropZone is null");
                }
                Destroy(movementGameObject);

            }

            GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;

            if (m_checkAnswer != null)
            {
                m_checkAnswer.GetComponent<Image>().color = Color.white;
                m_checkAnswer.GetComponent<CanvasGroup>().alpha = 0.0f;
                m_checkAnswer.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        // Remover cartas destruidas
        Cards.RemoveAll(x => x == null);

        // Actualizar index de las cartas
        foreach (var item in Cards)
        {
            item.GetComponent<DraggableTutorial>().index = Cards.IndexOf(item);
        }
    }

    IEnumerator CambiarAutoridadPiloto()
    {
        DisableButtons();
        globalTimerCanvasGroup.alpha = 0f;
        changeRolesCanvasGroup.alpha = 1f;
        teamInfoAnimator.SetBool("ChangeRole", true);
        yield return new WaitForSeconds(1f);

        string aux_nombre = NombrePiText.text;
        string aux_apellido = ApellidoPiText.text;

        NombrePiText.text = NombreCoText.text;
        ApellidoPiText.text = ApellidoCoText.text;

        NombreCoText.text = aux_nombre;
        ApellidoCoText.text = aux_apellido;

        string aux_nombre_pro = NombrePiProText.text;
        string aux_apellido_pro = ApellidoPiProText.text;

        NombrePiProText.text = NombreCoProText.text;
        ApellidoPiProText.text = ApellidoCoProText.text;

        NombreCoProText.text = aux_nombre_pro;
        ApellidoCoProText.text = aux_apellido_pro;
        yield return new WaitForSeconds(1f);
        teamInfoAnimator.SetBool("ChangeRole", false);
        globalTimerCanvasGroup.alpha = 0f;
        changeRolesCanvasGroup.alpha = 0f;
        terminoTutorial.alpha = 1f;
        terminoTutorial.blocksRaycasts = true;
        terminoTutorial.interactable = true;
    }

    IEnumerator HideAndShow()
    {

        HideAll();
        CameraControllerTutorial.Instance.Move(true);
        yield return new WaitForSeconds(1.0f);

        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (!teamObject.GetComponent<PlayerController3DTutorial>().restartPosition)
            {
                if (placeHolder.childCount > 0)
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
            else
            {
                break;
            }
        }

        while (teamObject.GetComponent<PlayerController3DTutorial>().restartPosition)
        {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1f);
        CameraControllerTutorial.Instance.Move(false);
        ShowInterface();
    }

    // Revisar 
    public void Revisar()
    {
        Debug.Log("Revisar");
        programmingAnimator.SetBool("WaitingPiloto2", true);
        buttonDebug.GetComponent<Button>().interactable = false;
        StartCoroutine(EndProgrammingTutorial());
    }

    IEnumerator EndProgrammingTutorial()
    {
        yield return new WaitForSeconds(5f);
        programmingAnimator.SetBool("EndProgrammingTutorial", true);
        Ejecutar();
    }

    public void ReadyForReview()
    {
        foreach (Transform movement in sequence)
        {
            GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;
            if (movement.transform.GetChild(0).childCount > 0)
            {
                movement.transform.GetChild(0).GetChild(0).GetComponent<DraggableTutorial>().enabled = false;
                m_checkAnswer.GetComponent<CanvasGroup>().alpha = 1.0f;
                m_checkAnswer.GetComponent<CanvasGroup>().blocksRaycasts = true;
            }
        }
        sequenceCanvasGroup.blocksRaycasts = true;
        buttonDebug.GetComponent<Button>().interactable = true;
        programmingAnimator.SetBool("Revisar1", true);
    }

    // Tutorial
    public void GoToRoles()
    {
        tutorialAnimator.SetBool("Roles", true);
    }

    public void GoToRolCopiloto()
    {
        tutorialAnimator.SetBool("RolCopiloto", true);
        IE_DisplayPeriodically = DisplayPeriodically();
        StartCoroutine(IE_DisplayPeriodically);
    }

    public void GoToFaseDePreguntas2Tutorial()
    {
        tutorialAnimator.SetBool("FaseDePreguntas2", true);
        isFirstQuestion = false;
        questionCard.blocksRaycasts = true;
        EnableButtons();
    }

    IEnumerator DisplayPeriodically()
    {
        yield return new WaitForSeconds(15f);

        while (isRespondingQuestions)
        {
            if (isDisplaying)
            {
                yield return new WaitForSeconds(12f);
            }
            else
            {
                Display();
            }
        }
    } 

    public void GoToRevisar2()
    {
        isFirstReview = false;
        programmingAnimator.SetBool("Revisar2", true);
    }

    public void GoToRevisar3()
    {
        programmingAnimator.SetBool("Revisar3", true);
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

    public void ExitTutorial()
    {
        popUpExitTutorial.alpha = 1.0f;
        popUpExitTutorial.blocksRaycasts = true;
    }

    public void PopUpExitTutorialNo()
    {
        popUpExitTutorial.alpha = 0.0f;
        popUpExitTutorial.blocksRaycasts = false;
    }

    public void PopUpExitTutorialYes()
    {
        TutorialManager.Instance.UnloadSceneCopiloto();
    }

    public void EnableProgramar()
    {
        buttonProgramar.GetComponent<Button>().interactable = true;
    }

    public void ShowInterface()
    {
        mainCanvasGroup.alpha = 1.0f;
        mainCanvasGroup.blocksRaycasts = true;
        questionCard.alpha = 0.0f;
        questionCard.blocksRaycasts = false;
        buttonsCanvasGroup.alpha = 1.0f;
        buttonsCanvasGroup.blocksRaycasts = true;
        teamInfoCanvasGroup.alpha = 1.0f;
        teamInfoCanvasGroup.blocksRaycasts = true;
        globalTimerCanvasGroup.alpha = 1f;
        globalTimerCanvasGroup.blocksRaycasts = true;
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
        waitingCopilotoDecision.alpha = 0;
        waitingCopilotoDecision.blocksRaycasts = false;
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
    }

    public void EnableResponder()
    {
        buttonResponder.GetComponent<Button>().interactable = true;
    }

    public void DisableResponder()
    {
        buttonResponder.GetComponent<Button>().interactable = false;
    }

    public void AddScore(int reward)
    {
        score += reward;
        ScoreText.text = score + "/2";
    }

    public void FirstObjectiveSuccessful()
    {
        checkBoxFirstObjective.sprite = successfulObjective;
    }

    public void SecondObjectiveSuccessful()
    {
        checkBoxSecondObjective.sprite = successfulObjective;
    }
}

