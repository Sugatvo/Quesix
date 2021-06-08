using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerPiloto : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Half, Finish };

    [Header("Tutorial")]
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup settingsCanvasGroup;
    [SerializeField] CanvasGroup popUpExitTutorial;
    [SerializeField] Animator tutorialAnimator = null;
    [SerializeField] Animator programmingAnimator = null;
    [SerializeField] GameObject teamObject;
    [SerializeField] Image checkBoxFirstObjective;
    [SerializeField] Image checkBoxSecondObjective;
    [SerializeField] Sprite successfulObjective;
    [SerializeField] CanvasGroup terminoTutorial;

    public bool isFirstQuestion;
    public bool isFirstAnswer;
    public bool isFirstProgramming;
    public bool isFirstDrag;
    public bool isFirstDrop;
    public bool isSelectedByTeammate;

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
    [SerializeField] Sprite successMov;
    [SerializeField] Sprite warningtMov;
    [SerializeField] Sprite incorrectMov;
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

    private static ManagerPiloto _instance;
    public static ManagerPiloto Instance { get { return _instance; } }

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
        LoadQuestionCards();
        score = 0;
        isFirstQuestion = true;
        isFirstProgramming = true;
        isFirstAnswer = true;
        isFirstDrag = true;
        isFirstDrop = true;
        resStateParaHash = Animator.StringToHash("ScreenState");
        timerStateParaHash = Animator.StringToHash("TimeState");
        timerDefaultColor = timerText.color;

        GameObject initialLeftCard = GameObject.Find("LeftCardTutorial");
        GameObject initialFowardCard = GameObject.Find("FowardCardTutorial");
        GameObject initialBackCard = GameObject.Find("BackCardTutorial");
        GameObject initialRightCard = GameObject.Find("RightCardTutorial");

        Cards.Add(initialLeftCard);
        initialLeftCard.GetComponent<DraggableTutorial>().index = Cards.IndexOf(initialLeftCard);
        Cards.Add(initialFowardCard);
        initialFowardCard.GetComponent<DraggableTutorial>().index = Cards.IndexOf(initialFowardCard);
        Cards.Add(initialBackCard);
        initialBackCard.GetComponent<DraggableTutorial>().index = Cards.IndexOf(initialBackCard);
        Cards.Add(initialRightCard);
        initialRightCard.GetComponent<DraggableTutorial>().index = Cards.IndexOf(initialRightCard);

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
        isSelectedByTeammate = false;

        if (isFirstQuestion)
        {
            tutorialAnimator.SetBool("FaseDeDecision", false);
            tutorialAnimator.SetBool("TransitionPreguntas", true);
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
        foreach(var answer in pickedAnswers.ToList())
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
        UptadeTimer(false);

        int CountCorrectAnswers = CompareAnswers();
        string correctAnswerText = GetCorrectAnswer();
        finishedQuestions.Add(currentQuestion);

        if (CountCorrectAnswers > 0)
        {
            for (int i = 0; i < CountCorrectAnswers; i++)
            {
                int random = Random.Range(0, 4);
                AddCard(random);
            }
        }

        ManagerPiloto.ResolutionScreenType type;

        if (CountCorrectAnswers == 2)
        {
            type = ManagerPiloto.ResolutionScreenType.Correct;
        }
        else if (CountCorrectAnswers == 1)
        {
            type = ManagerPiloto.ResolutionScreenType.Half;
        }
        else
        {
            type = ManagerPiloto.ResolutionScreenType.Incorrect;
        }

        DisplayResolutionScreen(type, CountCorrectAnswers, correctAnswerText);

        if (type != ManagerPiloto.ResolutionScreenType.Finish)
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
        }
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
            if (pickedAnswers.Count == 2)
            {
                EnableResponder();
            }
            else if (pickedAnswers.Count == 1 && isFirstAnswer)
            {
                RespuestaCopilotoTutorial();
                while (isFirstAnswer)
                {
                    yield return new WaitForSeconds(1.0f);
                }
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

    public void RandomOrCorrectAnswerSelection()
    {
        int rand_or_correct = Random.Range(0, 3);
        int index;
        if(rand_or_correct == 0)
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
        OnProgrammingWithAuthority();
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
    }

    public void OnProgrammingWithAuthority()
    {
        buttonRun.GetComponent<Button>().interactable = true;
        buttonDebug.GetComponent<Button>().interactable = true;
        handCanvasGroup.interactable = true;
        handCanvasGroup.blocksRaycasts = true;
        sequenceCanvasGroup.blocksRaycasts = true;
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
        
        programmingAnimator.SetBool("Revisar2", true);
        StartCoroutine(ReviewSequence());
        isCheck = true;
    }

    IEnumerator ReviewSequence()
    {
        yield return new WaitForSeconds(3.0f);
        foreach (Transform movement in sequence)
        {
            GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;
            if (movement.transform.GetChild(0).childCount > 0)
            {
                m_checkAnswer.GetComponent<CanvasGroup>().alpha = 1.0f;
                m_checkAnswer.GetComponent<CanvasGroup>().blocksRaycasts = true;
                m_checkAnswer.GetComponent<Image>().sprite = GetRandomReview();
            }
        }
        programmingAnimator.SetBool("Revisar3", true);
    }

    public Sprite GetRandomReview()
    {
        int rand = Random.Range(0, 2);

        if(rand == 0)
        {
            return successMov;
        }
        if(rand == 1)
        {
            return warningtMov;
        }
        else
        {
            return incorrectMov;
        }
    }

    // Tutorial
    public void StartTutorial()
    {
        tutorialAnimator.SetBool("Bienvenida", true);
    }

    public void InterfazTutorial()
    {
        tutorialAnimator.SetBool("Interfaz", true);
    }

    public void RolesTutorial()
    {
        tutorialAnimator.SetBool("Roles", true);
    }

    public void FaseDeDecisionTutorial()
    {
        tutorialAnimator.SetBool("FaseDeDecision", true);
    }

    public void FaseDePreguntasTutorial()
    {
        tutorialAnimator.SetBool("FaseDePreguntas", true);
        isFirstQuestion = false;
        questionCard.blocksRaycasts = true;
        EnableButtons();
    }

    public void RespuestaCopilotoTutorial()
    {
        tutorialAnimator.SetBool("RespuestaCopiloto", true);
    }

    public void RespuestaCopilotoFadeOutTutorial()
    {
        StartCoroutine(DisabledMainCanvasAnimator());
    }

    public void GoToInterfazTutorial()
    {
        programmingAnimator.SetBool("Interface", true);
        timerAnimator_Programming.SetInteger(timerStateParaHash, 2);
    }

    public void GoToPanelDeDigipasosTutorial()
    {
        timerAnimator_Programming.SetInteger(timerStateParaHash, 1);
        programmingAnimator.SetBool("PanelDeDigipasos", true);
    }

    public void GoToPanelDeProgramacionTutorial()
    {
        programmingAnimator.SetBool("PanelDeProgramacion", true);
    }

    public void GoToPanelDeProgramacion2Tutorial()
    {
        programmingAnimator.SetBool("PanelDeProgramacion2", true);
    }

    public void GoToArrastrarMovimientoTutorial()
    {
        timerAnimator_Programming.SetInteger(timerStateParaHash, 2);
        programmingAnimator.SetBool("ArrastrarMovimiento", true);
        isFirstProgramming = false;
    }

    public void GoToArrastrarMovimiento2Tutorial()
    {
        programmingAnimator.SetBool("ArrastrarMovimiento2", true);
    }

    public void GoToRevisar1Tutorial()
    {
        programmingAnimator.SetBool("Revisar1", true);
    }

    public void GoToMoverTutorial()
    {
        programmingAnimator.SetBool("Mover", true);
    }

    IEnumerator DisabledMainCanvasAnimator()
    {
        tutorialAnimator.SetBool("RespuestaCopiloto", false);
        yield return new WaitForSeconds(1.0f);
        isFirstAnswer = false;
        tutorialAnimator.enabled = false;
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
        TutorialManager.Instance.UnloadScenePiloto();
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
