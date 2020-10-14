using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Mirror;
using Cinemachine;
using TMPro;
using System.Data.Common;
using UnityEditor;

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
    [SerializeField] TextMeshProUGUI JugadorPiPoText;
    [SerializeField] TextMeshProUGUI JugadorCoPoText;

    [SerializeField] TextMeshProUGUI leftCountText;
    [SerializeField] TextMeshProUGUI fowardCountText;
    [SerializeField] TextMeshProUGUI backCountText;
    [SerializeField] TextMeshProUGUI rightCountText;

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
    [SerializeField] TextMeshProUGUI timerText_Programming = null;


    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;
    private IEnumerator IE_StartTimerProgramming = null;
    private IEnumerator IE_Ejecutar = null;
    private IEnumerator IE_HideAndShow = null;

    [Header("Cards")]
    [SerializeField] GameObject leftPrefab;
    [SerializeField] GameObject fowardPrefab;
    [SerializeField] GameObject backPrefab;
    [SerializeField] GameObject rightPrefab;

    [Space]

    [SerializeField] Transform leftSpawner;
    [SerializeField] Transform fowardSpawner;
    [SerializeField] Transform backSpawner;
    [SerializeField] Transform rightSpawner;

    public GameObject currentCardInMovement;

    [Space]
    [SyncVar]
    public int ownerID = -1;

    [SyncVar]
    public int teammateID = -1;

    private bool isFirstQuestion = false;

    void OnEnable()
    {
        Debug.Log("OnEnable: TeamManager");
        LoadMovementCards();

        JugadorPiText = GameObject.Find("Jugador_Pi").GetComponent<TextMeshProUGUI>();
        JugadorCoText = GameObject.Find("Jugador_Co").GetComponent<TextMeshProUGUI>();
        JugadorPiPoText = GameObject.Find("Jugador_Pi_Programming").GetComponent<TextMeshProUGUI>();
        JugadorCoPoText = GameObject.Find("Jugador_Co_Programming").GetComponent<TextMeshProUGUI>();
        cardCountText = GameObject.Find("CardCount").GetComponent<TextMeshProUGUI>();
        handContentArea = GameObject.Find("Hand").GetComponent<RectTransform>();
        sequence = GameObject.Find("Sequence").GetComponent<RectTransform>();
        responder = GameObject.Find("Responder").GetComponent<Button>();
        uiManager = GameObject.Find("Managers").GetComponent<UIManager>();
        timerAnimator = GameObject.Find("Timer").GetComponent<Animator>();
        timerText = GameObject.Find("Timer").transform.GetChild(0).gameObject.GetComponent<Text>();

        leftCountText = GameObject.Find("LeftCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        fowardCountText = GameObject.Find("FowardCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        backCountText = GameObject.Find("BackCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        rightCountText = GameObject.Find("RightCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        leftSpawner = GameObject.Find("LeftCards").transform;
        fowardSpawner = GameObject.Find("FowardCards").transform;
        backSpawner = GameObject.Find("BackCards").transform;
        rightSpawner = GameObject.Find("RightCards").transform;

        timerAnimator_Programming = GameObject.Find("Timer_Programming").GetComponent<Animator>();
        timerText_Programming = GameObject.Find("ContornoTimerProgramming").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

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
        events.AssignCard += AssignCard;
        events.ReassignCard += ReassignCard;
        events.CreteCardInstance += CreateCardInstance;
        events.SynchronizeOnBeginDrag += SynchronizeOnBeginDrag;
        events.SynchronizeOnDrag += SynchronizeOnDrag;
        events.SynchronizeOnDrop += SynchronizeOnDrop;
        events.SynchronizeOnEndDrag += SynchronizeOnEndDrag;
        responder.onClick.AddListener(() => AcceptForTeam());

        GetComponent<CameraController>().teamObject = teamObject;
        playerController = teamObject.GetComponent<PlayerController3D>();

        isFirstQuestion = true;

        if (!teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.DisableButtons();
            uiManager.SetRol(false, true);
            JugadorCoText.text = "*Jugador " + ownerID.ToString();
            JugadorCoText.fontSize += 5;
            JugadorCoPoText.text = "*Jugador " + ownerID.ToString();
            JugadorCoPoText.fontSize += 5;

            JugadorPiText.text = "Jugador " + teammateID.ToString();
            JugadorPiPoText.text = "Jugador " + teammateID.ToString();

        }
        else
        {
            uiManager.EnableButtons();
            uiManager.SetRol(true, false);
            JugadorPiText.text = "*Jugador " + ownerID.ToString();
            JugadorPiText.fontSize += 5;
            JugadorPiPoText.text = "*Jugador " + ownerID.ToString();
            JugadorPiPoText.fontSize += 5;

            JugadorCoText.text = "Jugador " + teammateID.ToString();
            JugadorCoPoText.text = "Jugador " + teammateID.ToString();
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
        events.AssignCard -= AssignCard;
        events.ReassignCard -= ReassignCard;
        events.CreteCardInstance -= CreateCardInstance;
        events.SynchronizeOnBeginDrag -= SynchronizeOnBeginDrag;
        events.SynchronizeOnDrag -= SynchronizeOnDrag;
        events.SynchronizeOnDrop -= SynchronizeOnDrop;
        events.SynchronizeOnEndDrag -= SynchronizeOnEndDrag;
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

        string aux2 = JugadorPiPoText.text;
        JugadorPiPoText.text = JugadorCoPoText.text;
        JugadorCoPoText.text = aux2;
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

        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority && int.Parse(cardCountText.text) >= 5)
        {
            uiManager.EnableProgramar();
        }
    }

    public void CreateCardInstance(int cardPrefabIndex)
    {
        Debug.Log("CreateCardInstance");
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdCreateCardInstance(cardPrefabIndex);
        }   
    }

    [Command]
    void CmdCreateCardInstance(int cardPrefabIndex)
    {
        Debug.Log("CmdCreateCardInstance");
        TargetCreateCardInstance(connectionToClient, cardPrefabIndex);
        TargetCreateCardInstance(teammate.connectionToClient, cardPrefabIndex);
    }

    [TargetRpc]
    public void TargetCreateCardInstance(NetworkConnection target, int card)
    {
        Debug.Log("TargetCreateCardInstance");

        if (card == 0)
        {
            currentCardInMovement = Instantiate(leftPrefab, leftSpawner, false);
        }
        else if (card == 1)
        {
            currentCardInMovement = Instantiate(fowardPrefab, fowardSpawner, false);
        }
        else if (card == 2)
        {
            currentCardInMovement = Instantiate(backPrefab, backSpawner, false);
        }
        else
        {
            currentCardInMovement = Instantiate(rightPrefab, rightSpawner, false);
        }
        teamObject.GetComponent<PlayerScoreQuesix>().Cards.Add(currentCardInMovement);

        currentCardInMovement.GetComponent<Draggable>().index = teamObject.GetComponent<PlayerScoreQuesix>().Cards.IndexOf(currentCardInMovement);
    }

    public void SynchronizeOnBeginDrag(int cardIndex)
    {
        Debug.Log("SynchronizeOnBeginDrag");
        CmdSynchronizeOnBeginDrag(cardIndex);
    }

    [Command]
    void CmdSynchronizeOnBeginDrag(int cardIndex)
    {
        Debug.Log("CmdSynchronizeOnBeginDrag");
        TargetSynchronizeOnBeginDrag(connectionToClient, cardIndex);
        TargetSynchronizeOnBeginDrag(teammate.connectionToClient, cardIndex);
    }

    [TargetRpc]
    public void TargetSynchronizeOnBeginDrag(NetworkConnection target, int cardIndex)
    {
        Debug.Log("TargetSynchronizeOnBeginDrag");
        teamObject.GetComponent<PlayerScoreQuesix>().Cards[cardIndex].GetComponent<Draggable>().SyncOnBeginDrag();
    }


    public void SynchronizeOnDrag(int cardIndex, Vector2 position, float oldX, float oldY)
    {
        Debug.Log("SynchronizeOnDrag");
        CmdSynchronizeOnDrag(cardIndex, position, oldX, oldY);
    }

    [Command]
    void CmdSynchronizeOnDrag(int cardIndex, Vector2 position, float oldX, float oldY)
    {
        Debug.Log("CmdSynchronizeOnBeginDrag");
        TargetSynchronizeOnDrag(connectionToClient, cardIndex, position, oldX, oldY);
        TargetSynchronizeOnDrag(teammate.connectionToClient, cardIndex, position, oldX, oldY);
    }

    [TargetRpc]
    public void TargetSynchronizeOnDrag(NetworkConnection target, int cardIndex, Vector2 position, float oldX, float oldY)
    {
        Debug.Log("TargetSynchronizeOnDrag");
        teamObject.GetComponent<PlayerScoreQuesix>().Cards[cardIndex].GetComponent<Draggable>().SyncOnDrag(position, oldX, oldY);
    }

    public void SynchronizeOnDrop(int cardIndex, int dropIndex)
    {
        Debug.Log("SynchronizeOnDrop");
        CmdSynchronizeOnDrop(cardIndex, dropIndex);
    }

    [Command]
    void CmdSynchronizeOnDrop(int cardIndex, int dropIndex)
    {
        Debug.Log("CmdSynchronizeOnDrop");
        TargetSynchronizeOnDrop(connectionToClient, cardIndex, dropIndex);
        TargetSynchronizeOnDrop(teammate.connectionToClient, cardIndex, dropIndex);
    }

    [TargetRpc]
    public void TargetSynchronizeOnDrop(NetworkConnection target, int cardIndex, int dropIndex)
    {
        Debug.Log("SynchronizeOnDrop");
        Draggable d = teamObject.GetComponent<PlayerScoreQuesix>().Cards[cardIndex].GetComponent<Draggable>();

        DropZone dropZone = teamObject.GetComponent<PlayerScoreQuesix>().dropZones[dropIndex].GetComponent<DropZone>();

        if (dropZone.currentMovement != null)
        {
            if (dropZone.currentMovement.CardAction.Equals("Izquierda"))
            {
                events.ReassignCard(0);
            }
            else if (dropZone.currentMovement.CardAction.Equals("Avanzar"))
            {
                events.ReassignCard(1);
            }
            else if (dropZone.currentMovement.CardAction.Equals("Retroceder"))
            {
                events.ReassignCard(2);
            }
            else
            {
                events.ReassignCard(3);
            }
            Destroy(dropZone.currentMovement.gameObject);
        } 
        d.parentToReturnTo = dropZone.transform;
        d.droppedOnSlot = true;
        dropZone.currentMovement = d.gameObject.GetComponent<MovCardData>();

        if (dropZone.currentMovement.CardAction.Equals("Izquierda"))
        {
            dropZone.movementAction.text = "Girar";
        }
        else if (dropZone.currentMovement.CardAction.Equals("Avanzar"))
        {
            dropZone.movementAction.text = "Avanzar";
        }
        else if (dropZone.currentMovement.CardAction.Equals("Retroceder"))
        {
            dropZone.movementAction.text = "Retroceder";
        }
        else
        {
            dropZone.movementAction.text = "Girar";
        }
    }

    public void SynchronizeOnEndDrag(int cardIndex)
    {
        Debug.Log("SynchronizeOnEndDrag");
        CmdSynchronizeOnEndDrag(cardIndex);
    }

    [Command]
    void CmdSynchronizeOnEndDrag(int cardIndex)
    {
        Debug.Log("CmdSynchronizeOnEndDrag");
        TargetSynchronizeOnEndDrag(connectionToClient, cardIndex);
        TargetSynchronizeOnEndDrag(teammate.connectionToClient, cardIndex);
    }

    [TargetRpc]
    public void TargetSynchronizeOnEndDrag(NetworkConnection target, int cardIndex)
    {
        Debug.Log("TargetSynchronizeOnEndDrag");
       
        Draggable d = teamObject.GetComponent<PlayerScoreQuesix>().Cards[cardIndex].GetComponent<Draggable>();

        Debug.Log("Card: " + d);

        Debug.Log("Validation droppedOnSlot = " + d.droppedOnSlot);
        if (d.droppedOnSlot)
        {
            // Verificar de donde viene y ver si quitar o no movimientos
            SpawnOnDrag Spawner = d.validationParent.GetComponent<SpawnOnDrag>();

            if (Spawner != null)
            {
                Debug.Log("Spawner drop on slot");
                MovCardData cardInfo = d.GetComponent<MovCardData>();
                if (cardInfo.CardAction.Equals("Izquierda"))
                {
                    events.AssignCard(0);
                }
                else if (cardInfo.CardAction.Equals("Avanzar"))
                {
                    events.AssignCard(1);
                }
                else if (cardInfo.CardAction.Equals("Retroceder"))
                {
                    events.AssignCard(2);
                }
                else
                {
                    events.AssignCard(3);
                }
            }
            else
            {
                Debug.Log("Dropzone drop on slot");
                DropZone dropZone = d.validationParent.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    dropZone.movementAction.text = string.Empty;
                }

                dropZone.currentMovement = null;

            }

            d.transform.SetParent(d.parentToReturnTo);
            d.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            d.canvasGroup.blocksRaycasts = true;
            d.canvasGroup.alpha = 1.0f;

            // Remover cartas destruidas
            teamObject.GetComponent<PlayerScoreQuesix>().Cards.RemoveAll(x => x == null);

            // Actualizar index de las cartas
            foreach (var item in teamObject.GetComponent<PlayerScoreQuesix>().Cards)
            {
                item.GetComponent<Draggable>().index = teamObject.GetComponent<PlayerScoreQuesix>().Cards.IndexOf(item);
            }


        }
        else
        {
            SpawnOnDrag Spawner = d.validationParent.GetComponent<SpawnOnDrag>();

            if (Spawner != null)
            {
                Debug.Log("Spawner dont drop on slot");
            }
            else
            {
                Debug.Log("Dropzone dont drop on slot");
                DropZone dropZone = d.validationParent.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    dropZone.movementAction.text = string.Empty;
                }

                MovCardData cardInfo = d.GetComponent<MovCardData>();
                if (cardInfo.CardAction.Equals("Izquierda"))
                {
                    events.ReassignCard(0);
                }
                else if (cardInfo.CardAction.Equals("Avanzar"))
                {
                    events.ReassignCard(1);
                }
                else if (cardInfo.CardAction.Equals("Retroceder"))
                {
                    events.ReassignCard(2);
                }
                else
                {
                    events.ReassignCard(3);
                }
            }

            Destroy(d.gameObject);

            // Remover cartas destruidas
            teamObject.GetComponent<PlayerScoreQuesix>().Cards.RemoveAll(x => x == null);

            foreach (var item in teamObject.GetComponent<PlayerScoreQuesix>().Cards)
            {
                item.GetComponent<Draggable>().index = teamObject.GetComponent<PlayerScoreQuesix>().Cards.IndexOf(item);
            }


        }
    }



    public void AssignCard(int index)
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdAssignCard(index);
        }

    }
    [Command]
    void CmdAssignCard(int index)
    {
        TargetAssign(connectionToClient, index);
        TargetAssign(teammate.connectionToClient, index);
    }

    [TargetRpc]
    public void TargetAssign(NetworkConnection target, int index)
    {
        if (index == 0)
        {
            leftCountText.text = (int.Parse(leftCountText.text) - 1).ToString();
        }
        else if (index == 1)
        {
            fowardCountText.text = (int.Parse(fowardCountText.text) - 1).ToString();
        }
        else if (index == 2)
        {
            backCountText.text = (int.Parse(backCountText.text) - 1).ToString();
        }
        else
        {
            rightCountText.text = (int.Parse(rightCountText.text) - 1).ToString();
        }

        cardCountText.text = (int.Parse(cardCountText.text) - 1).ToString();
    }

    public void ReassignCard(int index)
    {
        if (teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            CmdReassignCard(index);
        }

    }
    [Command]
    void CmdReassignCard(int index)
    {
        TargetReassign(connectionToClient, index);
        TargetReassign(teammate.connectionToClient, index);
    }

    [TargetRpc]
    public void TargetReassign(NetworkConnection target, int index)
    {
        if (index == 0)
        {
            leftCountText.text = (int.Parse(leftCountText.text) + 1).ToString();
        }
        else if (index == 1)
        {
            fowardCountText.text = (int.Parse(fowardCountText.text) + 1).ToString();
        }
        else if (index == 2)
        {
            backCountText.text = (int.Parse(backCountText.text) + 1).ToString();
        }
        else
        {
            rightCountText.text = (int.Parse(rightCountText.text) + 1).ToString();
        }

        cardCountText.text = (int.Parse(cardCountText.text) + 1).ToString();
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
            CmdDebuggingWithAuthority();
        }
        else
        {
            CmdDebuggingWithoutAuthority();
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
            uiManager.isCheck = true;
            uiManager.DisabledButtonDebug();
            uiManager.DisabledButtonRun();
            uiManager.DisabledMovement();
        }
        else
        {
            foreach (Transform movement in sequence)
            {
                GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;
                if(movement.transform.GetChild(0).childCount > 0)
                {
                    movement.transform.GetChild(0).GetChild(0).GetComponent<Draggable>().enabled = false;
                    m_checkAnswer.GetComponent<CanvasGroup>().alpha = 1.0f;
                    m_checkAnswer.GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
            }
            uiManager.EnabledChangeColor();
            uiManager.EnabledButtonDebug();
        }
    }

    [Command]
    void CmdDebuggingWithoutAuthority()
    {
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
            uiManager.EnabledMovement();
        }
        else
        {
            int i = 0;
            foreach (Transform movement in sequence)
            {
                GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;
                if (movement.transform.GetChild(0).childCount > 0)
                {
                    CmdSyncColor(i, m_checkAnswer.GetComponent<Image>().color);
                }
                i += 1;
            }
            uiManager.DisabledButtonDebug();
            uiManager.DisabledButtonRun();
            uiManager.DisabledMovement();
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
        foreach (Transform movement in sequence)
        {
            GameObject m_checkAnswer = movement.transform.GetChild(2).gameObject;
            if (index == i)
            {
                m_checkAnswer.GetComponent<Image>().color = cardColor;
                m_checkAnswer.GetComponent<CanvasGroup>().alpha = 1.0f;
                m_checkAnswer.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            i += 1;
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
            if (isFirstQuestion)
            {
                Debug.Log("isFirstQuestion: " + isFirstQuestion);
                // Si es la primera vez, mostrar tutorial y luego iniciar cronometro
                Animator tutorialAnimator = uiManager.transform.GetComponent<TutorialManager>().m_Animator;
                if(tutorialAnimator != null)
                {
                    tutorialAnimator.SetBool("isFirstQuestion", true);
                }
                else
                {
                    Debug.Log("tutorialAnimator is null");
                }
                
                UptadeTimerFirstTime(pregunta.UseTimer);
                Debug.Log("UpdateTimerFirstTime");
                isFirstQuestion = false;
            }
            else
            {
                UptadeTimer(pregunta.UseTimer);
                Debug.Log("UpdateTimer");
            }
        }
    }


    public void Accept()
    {
        if (isFirstQuestion)
        {
            UptadeTimerFirstTime(false);
        }
        else
        {
            UptadeTimer(false);
        }
        
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

    void UptadeTimerFirstTime(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimer = StartTimerFirstQuestion();
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

    IEnumerator StartTimerFirstQuestion()
    {
        uiManager.DisableQuestion();
        var waitTime = 9.0f;
        while (waitTime > 0)
        {
            waitTime--;
            yield return new WaitForSeconds(1.0f);
        }
        uiManager.EnableQuestion();

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
        // Mostrar ui denuevo
        uiManager.ShowInterface();
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(1.0f);
        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if(placeHolder.childCount > 0)
            {
                MovCardData card = placeHolder.GetChild(0).GetComponent<MovCardData>();
                if (card.CardAction.Equals("Avanzar"))
                {
                    teamObject.GetComponent<PlayerController3D>().Avanzar();
                }
                else if (card.CardAction.Equals("Retroceder"))
                {
                    teamObject.GetComponent<PlayerController3D>().Retroceder();
                }
                else if (card.CardAction.Equals("Izquierda"))
                {
                    teamObject.GetComponent<PlayerController3D>().GirarIzquierda();
                }
                else
                {
                    teamObject.GetComponent<PlayerController3D>().GirarDerecha();
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
        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (placeHolder.childCount > 0)
            {
                GameObject movementGameObject = placeHolder.GetChild(0).gameObject;
                DropZone dropZone = placeHolder.GetComponent<DropZone>();
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
        teamObject.GetComponent<PlayerScoreQuesix>().Cards.RemoveAll(x => x == null);

        // Actualizar index de las cartas
        foreach (var item in teamObject.GetComponent<PlayerScoreQuesix>().Cards)
        {
            item.GetComponent<Draggable>().index = teamObject.GetComponent<PlayerScoreQuesix>().Cards.IndexOf(item);
        }

        // cardCountText.text = $"{handContentArea.childCount}";
    }


    IEnumerator HideAndShow()
    {
        
        uiManager.HideAll();
        events.PlayerIsMoving(true);
        yield return new WaitForSeconds(1.0f);

        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (placeHolder.childCount > 0)
            {
                yield return new WaitForSeconds(1.0f);
            }
        }
        yield return new WaitForSeconds(1.0f);
        events.PlayerIsMoving(false);
        uiManager.ShowInterface();   
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