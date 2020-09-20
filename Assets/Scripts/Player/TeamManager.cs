using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;
using TMPro;


[System.Serializable]
public struct AnswerID
{
    public int answerIndex;
    public bool p1;
    public bool p2;
}

public class TeamManager : NetworkBehaviour
{
    [Header("Team Information")]
    [SyncVar]
    public GameObject teamObject;

    [SyncVar]
    public NetworkIdentity teammate;

    private PlayerController3D playerController;

    [Header("UI Elements(Prefabs)")]
    [SerializeField] MovCardData cardPrefab = null;
    [SerializeField] RectTransform handContentArea;
    [SerializeField] RectTransform sequence;
    [SerializeField] TextMeshProUGUI cardCountText;
    [SerializeField] TextMeshProUGUI JugadorPiText;
    [SerializeField] TextMeshProUGUI JugadorCoText;

    MovementCard[] _movimientos = null;
    public MovementCard[] Movimientos { get { return _movimientos; } }

    [SerializeField] GameEvents events = null;
    [SerializeField] UIManager uiManager = null;
    [SerializeField] Button responder = null;

    [SerializeField] Animator timerAnimator = null;
    [SerializeField] Text timerText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor = Color.white;
    private int timerStateParaHash = 0;

    [SerializeField] Animator timerAnimator_Programming = null;
    [SerializeField] Text timerText_Programming = null;


    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;
    private IEnumerator IE_StartTimerProgramming = null;
    private IEnumerator IE_Ejecutar = null;
    private IEnumerator IE_HideAndShow = null;


    [SyncVar]
    public int ownerID = -1;

    [SyncVar]
    public int teammateID = -1;

    void OnEnable()
    {
        Debug.Log("OnEnable: TeamManager");
        LoadMovementCards();

        JugadorPiText = GameObject.Find("Jugador_Pi").GetComponent<TextMeshProUGUI>();
        JugadorCoText = GameObject.Find("Jugador_Co").GetComponent<TextMeshProUGUI>();
        cardCountText = GameObject.Find("CardCount").GetComponent<TextMeshProUGUI>();
        handContentArea = GameObject.Find("Hand").GetComponent<RectTransform>();
        sequence = GameObject.Find("Sequence").GetComponent<RectTransform>();
        responder = GameObject.Find("Responder").GetComponent<Button>();
        uiManager = GameObject.Find("Managers").GetComponent<UIManager>();
        timerAnimator = GameObject.Find("Timer").GetComponent<Animator>();
        timerText = GameObject.Find("Timer").transform.GetChild(0).gameObject.GetComponent<Text>();

        timerAnimator_Programming = GameObject.Find("Timer_Programming").GetComponent<Animator>();
        timerText_Programming = GameObject.Find("Timer_Programming").transform.GetChild(0).gameObject.GetComponent<Text>();

        timerDefaultColor = timerText.color;
        timerStateParaHash = Animator.StringToHash("TimeState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer: TeamManager");
        events.DisplayQuestion += DisplayForTeam;
        events.DisplayProgramar += ProgramarForTeam;
        events.SelectAnswer += SelectAnswer;
        events.AcceptAnswer += Accept;
        events.ShowQuestion += Display;
        events.Ejecutar += MovePlayer;
        events.Debug += Debugging;
        responder.onClick.AddListener(() => AcceptForTeam());

        GetComponent<CameraController>().teamObject = teamObject;
        playerController = teamObject.GetComponent<PlayerController3D>();

        if (!teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.DisableButtons();
            JugadorCoText.text = "*Jugador " + ownerID.ToString();
            JugadorPiText.text = "Jugador " + teammateID.ToString();

        }
        else
        {
            uiManager.EnableButtons();
            JugadorPiText.text = "*Jugador " + ownerID.ToString();
            JugadorCoText.text = "Jugador " + teammateID.ToString();
        }
    }


    void OnDisable()
    {
        events.DisplayQuestion -= DisplayForTeam;
        events.DisplayProgramar -= ProgramarForTeam;
        events.SelectAnswer -= SelectAnswer;
        events.AcceptAnswer -= AcceptForTeam;
        events.ShowQuestion -= Display;
        events.Ejecutar -= MovePlayer;
        responder.onClick.RemoveListener(() => AcceptForTeam());
    }

    [Command]
    void CmdCambiarAutoridad()
    {
        teamObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        teamObject.GetComponent<NetworkIdentity>().AssignClientAuthority(teammate.connectionToClient);

        TargetChangeAuthority(connectionToClient);
        TargetChangeAuthority(teammate.connectionToClient);

    }

    [TargetRpc]
    public void TargetChangeAuthority(NetworkConnection target)
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.EnableButtons();
        }
        else
        {
            uiManager.DisableButtons();
        }

        string aux = JugadorPiText.text;
        JugadorPiText.text = JugadorCoText.text;
        JugadorCoText.text = aux;
    }

    public void MovePlayer()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdHideUI();
            IE_Ejecutar = Sequence();
            StartCoroutine(IE_Ejecutar);
        }
    }

    [Command]
    void CmdHideUI()
    {
        TargetHide(connectionToClient);
        TargetHide(teammate.connectionToClient);
    }

    [TargetRpc]
    public void TargetHide(NetworkConnection target)
    {
        UptadeTimerProgramming(false);
        IE_HideAndShow = HideAndShow();
        StartCoroutine(IE_HideAndShow);
    }

    public void AddCard()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdAddCard();
        }

    }
    [Command]
    void CmdAddCard()
    {
        int random = UnityEngine.Random.Range(0, Movimientos.Length);
        TargetAdd(connectionToClient, random);
        TargetAdd(teammate.connectionToClient, random);
    }

    [TargetRpc]
    public void TargetAdd(NetworkConnection target, int randomIndex)
    {
        MovementCard card = Movimientos[randomIndex];
        MovCardData newCard = Instantiate(cardPrefab, handContentArea);
        newCard.UpdateData(card.Titulo, card.Arrow);
        newCard.GetComponent<ChangeColor>().enabled = false;
        cardCountText.text = $"{handContentArea.childCount}";

        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority && int.Parse(cardCountText.text) >= 5)
        {
            uiManager.EnableProgramar();
        }
    }

    void AcceptForTeam()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdAcceptQuestion();
        }
    }

    [Command]
    void CmdAcceptQuestion()
    {
        TargetAccept(teammate.connectionToClient);
        TargetAccept(connectionToClient);
    }

    [TargetRpc]
    public void TargetAccept(NetworkConnection target)
    {
        events.AcceptAnswer();
    }

    void DisplayForTeam()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            int randomIndex = GetRandomQuestionIndex();
            CmdDisplayQuestion(randomIndex);
        }
    }

    [Command]
    void CmdDisplayQuestion(int randInt)
    {
        TargetClient(connectionToClient, randInt);
        TargetClient(teammate.connectionToClient, randInt);   
    }

    [TargetRpc]
    public void TargetClient(NetworkConnection target, int randomIndex)
    {
        uiManager.ShowQuestion();
        events.ShowQuestion(randomIndex);
    }

    void ProgramarForTeam()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdDisplayProgramar();
        }
    }

    [Command]
    void CmdDisplayProgramar()
    {
        TargetProgramar(connectionToClient);
        TargetProgramar(teammate.connectionToClient);
    }

    [TargetRpc]
    public void TargetProgramar(NetworkConnection target)
    {
        uiManager.ShowCanvas();

        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.OnProgrammingWithAuthority();
        }
        else
        {
            uiManager.OnProgrammingWithoutAuthority();
        }
        UptadeTimerProgramming(true);
    }

    public void Debugging()
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            foreach (Transform child in sequence)
            {
                MovCardData card = child.GetComponent<MovCardData>();
                Vector3 cardPosition = card.GetComponent<RectTransform>().position;
                if (card != null)
                {
                    CmdSyncSequence(cardPosition, card.TitleText.text);
                }
            }

            CmdDebuggingWithAuthority();
        }
        else
        {
            //teamObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            //teamObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

            int i = 0;
            foreach (Transform child in sequence)
            {
                MovCardData card = child.GetComponent<MovCardData>();
                Color cardColor = card.GetComponent<Image>().color;
                if (card != null)
                {
                    card.GetComponent<ChangeColor>().enabled = false;
                    CmdSyncColor(i, cardColor);  
                }
                i += 1;
            }
            CmdDebuggingWithoutAuthority();
        }
    }

    [Command]
    void CmdSyncSequence(Vector3 cardPosition, string cardName)
    {
        TargetSyncSequence(teammate.connectionToClient, cardPosition, cardName);
    }

    [TargetRpc]
    public void TargetSyncSequence(NetworkConnection target, Vector3 cardPosition, string cardName)
    {
        foreach (Transform child in handContentArea)
        {
            MovCardData card = child.GetComponent<MovCardData>();
            if(card.TitleText.text == cardName)
            {
                card.transform.position = cardPosition;
                card.transform.SetParent(sequence);
                card.GetComponent<Draggable>().enabled = false;
                card.GetComponent<ChangeColor>().enabled = true;
                break;
            }
        }
    }

    [Command]
    void CmdSyncColor(int index, Color cardColor)
    {
        TargetSyncColor(teammate.connectionToClient, index, cardColor);
    }

    [TargetRpc]
    public void TargetSyncColor(NetworkConnection target, int index, Color cardColor)
    {
        int i = 0;
        foreach (Transform child in sequence)
        {
            MovCardData card = child.GetComponent<MovCardData>();
            if (index == i)
            {
                card.GetComponent<Image>().color = cardColor;
                card.GetComponent<Draggable>().enabled = true;
                card.GetComponent<ChangeColor>().enabled = false;
                break;
            }
            i += 1;
        }
    }

    [Command]
    void CmdDebuggingWithoutAuthority()
    {
        //teamObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        //teamObject.GetComponent<NetworkIdentity>().AssignClientAuthority(teammate.connectionToClient);


        TargetReadyToRun(connectionToClient);
        TargetReadyToRun(teammate.connectionToClient);
    }

    [TargetRpc]
    public void TargetReadyToRun(NetworkConnection target)
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.DisabledButtonDebug();
            uiManager.EnabledButtonRun();
        }
        else
        {
            uiManager.DisabledButtonDebug();
            uiManager.DisabledButtonRun();
        }
    }

    [Command]
    void CmdDebuggingWithAuthority()
    {
        TargetDebugging(connectionToClient);
        TargetDebugging(teammate.connectionToClient);
    }

    [TargetRpc]
    public void TargetDebugging(NetworkConnection target)
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.DisabledButtonDebug();
        }
        else
        {
            uiManager.EnabledButtonDebug();
        }
    }



    public void SelectAnswer(int AnswerIndex)
    {
        CmdSelectAnswer(AnswerIndex);
    }

    [Command]
    void CmdSelectAnswer(int AnswerIndex)
    {
        int type = connectionToClient.identity.gameObject.GetComponent<CameraController>().type;

        TargetSelection(teammate.connectionToClient, AnswerIndex, type);
        TargetSelection(connectionToClient, AnswerIndex, type);

    }

    [TargetRpc]
    public void TargetSelection(NetworkConnection target, int AnswerIndex, int type)
    {
        if (type == 1)
        {
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer1 = true;
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer2 = false;
        }
        if (type == 2)
        {
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer1 = false;
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer2 = true;
        }
        uiManager.CurrentAnswers[AnswerIndex].UpdateAnswersUI();
        teamObject.GetComponent<PlayerScoreQuesix>().UpdateQuestionCardAnswer(uiManager.CurrentAnswers[AnswerIndex]);
    }


    public void EraseAnswers()
    {
        teamObject.GetComponent<PlayerScoreQuesix>().PickedAnswers = new List<AnswerID>();
    }

    int GetRandomQuestionIndex()
    {
        int random = 0;
        if (teamObject.GetComponent<PlayerScoreQuesix>().FinishedQuestions.Count < teamObject.GetComponent<PlayerScoreQuesix>().Preguntas.Length)
        {
            do
            {
                random = UnityEngine.Random.Range(0, teamObject.GetComponent<PlayerScoreQuesix>().Preguntas.Length);
            } while (teamObject.GetComponent<PlayerScoreQuesix>().FinishedQuestions.Contains(random) || random == teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion);
        }
        return random;
    }

    QuestionCard GetRandomQuestion(int randomIndex)
    {
        teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion = randomIndex;
        return teamObject.GetComponent<PlayerScoreQuesix>().Preguntas[teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion];
    }

    public void Display(int rand)
    {
        EraseAnswers();
        QuestionCard pregunta = GetRandomQuestion(rand);

        if (events.UpdateQuestionCardUI != null)
        {
            events.UpdateQuestionCardUI(pregunta);
        }
        else
        {
            Debug.LogWarning("Ocurrió un error al mostrar una nueva carta de pregunta.");
        }

        if (pregunta.UseTimer)
        {
            UptadeTimer(pregunta.UseTimer);
        }
    }


    public void Accept()
    {
        UptadeTimer(false);
        int CountCorrectAnswers = teamObject.GetComponent<PlayerScoreQuesix>().CompareAnswers();
        teamObject.GetComponent<PlayerScoreQuesix>().FinishedQuestions.Add(teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion);

        bool isCorrect;
        if (CountCorrectAnswers > 0)
        {
            for (int i = 0; i < CountCorrectAnswers; i++)
            {
                AddCard();
            }
            isCorrect = true;
        }
        else isCorrect = false;

        var type = (isCorrect) ? UIManager.ResolutionScreenType.Correct : UIManager.ResolutionScreenType.Incorrect;

        events.DisplayResolutionScreen(type, CountCorrectAnswers);

        if (type != UIManager.ResolutionScreenType.Finish)
        {
            if (IE_WaitTillNextRound != null)
            {
                StopCoroutine(IE_WaitTillNextRound);
            }
            IE_WaitTillNextRound = WaitTillNextRound();
            StartCoroutine(IE_WaitTillNextRound);
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
        var totalTime = teamObject.GetComponent<PlayerScoreQuesix>().Preguntas[teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion].Timer;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while (timeLeft > 0)
        {
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
        Accept();
    }



    void UptadeTimerProgramming(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimerProgramming = StartTimerProgramming();
                StartCoroutine(IE_StartTimerProgramming);

                timerAnimator_Programming.SetInteger(timerStateParaHash, 2);
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
        var totalTime = 60;
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
        uiManager.OnClickEjecutar();
    }


    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        // Mostrar denuevo los botones
        uiManager.ShowButtons();
    }

    IEnumerator Sequence()
    {
        foreach (Transform child in sequence)
        {
            MovCardData card = child.GetComponent<MovCardData>();
            if (card != null)
            {

                if (card.TitleText.text.Equals("Avanzar"))
                {
                    teamObject.GetComponent<PlayerController3D>().Up = true;
                }
                else if (card.TitleText.text.Equals("Retroceder"))
                {
                    teamObject.GetComponent<PlayerController3D>().Down = true;
                }
                else if (card.TitleText.text.Equals("Izquierda"))
                {
                    teamObject.GetComponent<PlayerController3D>().GirarIzq = true;
                }
                else
                {
                    teamObject.GetComponent<PlayerController3D>().GirarDer = true;
                }
                yield return new WaitForSeconds(1.2f);
            }
        }
        CmdBorrarCartas();
        CmdCambiarAutoridad();
    }

    [Command]
    void CmdBorrarCartas()
    {
        TargetBorrarCartas(connectionToClient);
        TargetBorrarCartas(teammate.connectionToClient);
    }

    [TargetRpc]
    public void TargetBorrarCartas(NetworkConnection target)
    {
        foreach (Transform item in sequence)
        {
            Destroy(item.gameObject);
        }
        cardCountText.text = $"{handContentArea.childCount}";
    }


    IEnumerator HideAndShow()
    {
        uiManager.HideAll();
        foreach (Transform child in sequence)
        {
            MovCardData card = child.GetComponent<MovCardData>();
            if (card != null)
            {
                yield return new WaitForSeconds(1.0f);
            }
        }
        yield return new WaitForSeconds(1.0f);
        uiManager.ShowButtons();
    }


    void LoadMovementCards()
    {
        Object[] objs = Resources.LoadAll("Cartas/Movimientos", typeof(MovementCard));

        _movimientos = new MovementCard[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            _movimientos[i] = (MovementCard)objs[i];
        }

        _movimientos = _movimientos.Select(r => (r as MovementCard)).Where(r => r != null).OrderBy(t => t.ID).ToArray<MovementCard>();
    }


}