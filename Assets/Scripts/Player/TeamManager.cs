using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;
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
    [Header("Owner Information")]
    public GameObject lobbyPlayer;


    [Header("Team Information")]
    [SyncVar]
    public GameObject teamObject;

    [SyncVar]
    public NetworkIdentity teammate;

    [SyncVar]
    public string matchID;

    [Header("UI Elements(Prefabs)")]
    [SerializeField] MovCardData cardPrefab = null;
    [SerializeField] RectTransform handContentArea;
    [SerializeField] RectTransform sequence;
    [SerializeField] TextMeshProUGUI cardCountText;
    [SerializeField] TextMeshProUGUI NombrePiText;
    [SerializeField] TextMeshProUGUI ApellidoPiText;
    [SerializeField] TextMeshProUGUI NombreCoText;
    [SerializeField] TextMeshProUGUI ApellidoCoText;
    [SerializeField] TextMeshProUGUI NombrePiProText;
    [SerializeField] TextMeshProUGUI ApellidoPiProText;
    [SerializeField] TextMeshProUGUI NombreCoProText;
    [SerializeField] TextMeshProUGUI ApellidoCoProText;
    [SerializeField] TextMeshProUGUI TeamNameText;
    [SerializeField] TextMeshProUGUI TeamNameProText;
    [SerializeField] Image RatonIcon;
    [SerializeField] Image RatonIconPro;

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
    [SerializeField] TextMeshProUGUI timerText = null;
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

    [SyncVar] public string nombre_owner;
    [SyncVar] public string apellido_owner;

    [SyncVar] public string nombre_teammate;
    [SyncVar] public string apellido_teammate;

    void OnEnable()
    {
        Debug.Log("OnEnable: TeamManager");
        LoadMovementCards();

        NombrePiText = GameObject.Find("Nombre_Pi").GetComponent<TextMeshProUGUI>();
        ApellidoPiText = GameObject.Find("Apellido_Pi").GetComponent<TextMeshProUGUI>();
        NombreCoText = GameObject.Find("Nombre_Co").GetComponent<TextMeshProUGUI>();
        ApellidoCoText = GameObject.Find("Apellido_Co").GetComponent<TextMeshProUGUI>();
        NombrePiProText = GameObject.Find("Nombre_Pi_Pro").GetComponent<TextMeshProUGUI>();
        ApellidoPiProText = GameObject.Find("Apellido_Pi_Pro").GetComponent<TextMeshProUGUI>();
        NombreCoProText = GameObject.Find("Nombre_Co_Pro").GetComponent<TextMeshProUGUI>();
        ApellidoCoProText = GameObject.Find("Apellido_Co_Pro").GetComponent<TextMeshProUGUI>();
        TeamNameText = GameObject.Find("TeamName").GetComponent<TextMeshProUGUI>();
        TeamNameProText = GameObject.Find("TeamNamePro").GetComponent<TextMeshProUGUI>();
        cardCountText = GameObject.Find("CardCount").GetComponent<TextMeshProUGUI>();
        handContentArea = GameObject.Find("Hand").GetComponent<RectTransform>();
        sequence = GameObject.Find("Sequence").GetComponent<RectTransform>();
        responder = GameObject.Find("Responder").GetComponent<Button>();
        uiManager = GameObject.Find("Managers").GetComponent<UIManager>();
        timerAnimator = GameObject.Find("Timer").GetComponent<Animator>();
        timerText = GameObject.Find("Timer").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        leftCountText = GameObject.Find("LeftCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        fowardCountText = GameObject.Find("FowardCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        backCountText = GameObject.Find("BackCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        rightCountText = GameObject.Find("RightCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        leftSpawner = GameObject.Find("LeftCards").transform;
        fowardSpawner = GameObject.Find("FowardCards").transform;
        backSpawner = GameObject.Find("BackCards").transform;
        rightSpawner = GameObject.Find("RightCards").transform;

        timerAnimator_Programming = GameObject.Find("Timer_Programming").GetComponent<Animator>();
        timerText_Programming = GameObject.Find("Timer_Programming").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        timerDefaultColor = timerText.color;
        timerStateParaHash = Animator.StringToHash("TimeState");

        var seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);
       
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
        events.CreateCardInstance += CreateCardInstance;
        events.SynchronizeOnBeginDrag += SynchronizeOnBeginDrag;
        events.SynchronizeOnDrag += SynchronizeOnDrag;
        events.SynchronizeOnDrop += SynchronizeOnDrop;
        events.SynchronizeOnEndDrag += SynchronizeOnEndDrag;
        events.GoBackToLobby += ReturnToLobby;
        responder.onClick.AddListener(() => AcceptForTeam());

        TeamNameText.text = "Equipo " + teamObject.GetComponent<PlayerScoreQuesix>().id_team.ToString();
        TeamNameProText.text = "Equipo " + teamObject.GetComponent<PlayerScoreQuesix>().id_team.ToString();
        RatonIcon = GameObject.Find("RatonIconMain").GetComponent<Image>();
        RatonIcon.color = teamObject.GetComponent<PlayerScoreQuesix>().objectColor;

        RatonIconPro = GameObject.Find("RatonIconPro").GetComponent<Image>();
        RatonIconPro.color = teamObject.GetComponent<PlayerScoreQuesix>().objectColor;

        if (!teamObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            uiManager.DisableButtons();
            NombreCoText.text = nombre_owner;
            ApellidoCoText.text = apellido_owner;
            NombreCoProText.text = nombre_owner;
            ApellidoCoProText.text = apellido_owner;

            NombrePiText.text = nombre_teammate;
            ApellidoPiText.text = apellido_teammate;
            NombrePiProText.text = nombre_teammate;
            ApellidoPiProText.text = apellido_teammate;
        }
        else
        {
            uiManager.EnableButtons();
            NombrePiText.text = nombre_owner;
            ApellidoPiText.text = apellido_owner;
            NombrePiProText.text = nombre_owner;
            ApellidoPiProText.text = apellido_owner;

            NombreCoText.text = nombre_teammate;
            ApellidoCoText.text = apellido_teammate;
            NombreCoProText.text = nombre_teammate;
            ApellidoCoProText.text = apellido_teammate;
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
        events.CreateCardInstance -= CreateCardInstance;
        events.SynchronizeOnBeginDrag -= SynchronizeOnBeginDrag;
        events.SynchronizeOnDrag -= SynchronizeOnDrag;
        events.SynchronizeOnDrop -= SynchronizeOnDrop;
        events.SynchronizeOnEndDrag -= SynchronizeOnEndDrag;
        events.GoBackToLobby -= ReturnToLobby;
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
            if(int.Parse(cardCountText.text) >= 5)
            {
                uiManager.EnableProgramar();
            }
            
        }
        else
        {
            uiManager.DisableButtons();
        }

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
        CmdCreateCardInstance(cardPrefabIndex);
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
        
        // Si el dropzone ya tiene un movimiento, removerlo y asignar el nuevo
        if (dropZone.currentMovement != null)
        {
            // Restar -1 al contador de la carta correspondiente
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
        
        // Si la carta fue soltada dentor de un dropzone
        if (d.droppedOnSlot)
        {
            // Verificar de donde viene y ver si quitar o no movimientos
            SpawnOnDrag Spawner = d.validationParent.GetComponent<SpawnOnDrag>();

            if (Spawner != null)
            {
                Debug.Log("Spawner drop on slot");

                // Restar -1 al texto de la carta correspondiente
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
                // Viene de dropzone, por lo tanto no se resta al contador
                Debug.Log("Dropzone drop on slot");
                DropZone dropZone = d.validationParent.GetComponent<DropZone>();

                // Borrar texto del dropzone anterior
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
        // Si la carta fue soltada fuera de un dropzone
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

                // Agregar +1 al texto de la carta correspondiente
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

    IEnumerator WaitForQuestionsCreator(int randomIndex)
    {
        while (teamObject.GetComponent<PlayerScoreQuesix>().questions_loading)
        {
            yield return new WaitForEndOfFrame();
        }
        uiManager.ShowQuestion();
        events.ShowQuestion(randomIndex);
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
        StartCoroutine(WaitForQuestionsCreator(randomIndex));
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
                if (movement.transform.GetChild(0).childCount > 0)
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
        TargetSelection(connectionToClient, AnswerIndex, 1);
        TargetSelection(teammate.connectionToClient, AnswerIndex, 2);
    }

    [TargetRpc]
    public void TargetSelection(NetworkConnection target, int AnswerIndex, int type)
    {
        // Owner
        if (type == 1)
        {
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer1 = true;
            uiManager.CurrentAnswers[AnswerIndex].SelectionPlayer2 = false;
        }
        // Teammate
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
                random = Random.Range(0, teamObject.GetComponent<PlayerScoreQuesix>().Preguntas.Length);
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
            Debug.Log("UpdateTimer");
        }
    }


    public void Accept()
    {
        UptadeTimer(false);

        int CountCorrectAnswers = teamObject.GetComponent<PlayerScoreQuesix>().CompareAnswers();
        string correctAnswerText = teamObject.GetComponent<PlayerScoreQuesix>().GetCorrectAnswer();
        teamObject.GetComponent<PlayerScoreQuesix>().FinishedQuestions.Add(teamObject.GetComponent<PlayerScoreQuesix>().CurrentQuestion);

        if (CountCorrectAnswers > 0)
        {
            for (int i = 0; i < CountCorrectAnswers; i++)
            {
                AddCard();
            }
        }

        UIManager.ResolutionScreenType type;
        
        if (CountCorrectAnswers == 2)
        {
            type = UIManager.ResolutionScreenType.Correct;
        }
        else if (CountCorrectAnswers == 1)
        {
            type = UIManager.ResolutionScreenType.Half;
        }
        else
        {
            type = UIManager.ResolutionScreenType.Incorrect;
        }

        events.DisplayResolutionScreen(type, CountCorrectAnswers, correctAnswerText);

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
        uiManager.TimeOutProgramming();
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

            if (placeHolder.childCount > 0)
            {
                MovCardData card = placeHolder.GetChild(0).GetComponent<MovCardData>();

                if (!teamObject.GetComponent<PlayerController3D>().restartPosition)
                {
                    CmdMove(card.CardAction);
                    yield return new WaitForSeconds(1.2f);
                }
                else
                {
                    break;
                }
            }
        }
        while (teamObject.GetComponent<PlayerController3D>().restartPosition)
        {
            yield return new WaitForSeconds(1f);
        }
        CmdBorrarCartas();
        CmdCambiarAutoridad();
    }

    [Command]
    void CmdMove(string _movimiento)
    {
        TargetRpcMove(connectionToClient, _movimiento);
        TargetRpcMove(teammate.connectionToClient, _movimiento);
    }

    [TargetRpc]
    public void TargetRpcMove(NetworkConnection target, string _movimiento)
    {
        if (_movimiento.Equals("Avanzar"))
        {
            teamObject.GetComponent<PlayerController3D>().Avanzar();
        }
        else if (_movimiento.Equals("Retroceder"))
        {
            teamObject.GetComponent<PlayerController3D>().Retroceder();
        }
        else if (_movimiento.Equals("Izquierda"))
        {
            teamObject.GetComponent<PlayerController3D>().GirarIzquierda();
        }
        else
        {
            teamObject.GetComponent<PlayerController3D>().GirarDerecha();
        }
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
    }


    IEnumerator HideAndShow()
    {

        uiManager.HideAll();
        events.PlayerIsMoving(true);
        yield return new WaitForSeconds(1.0f);

        foreach (Transform movement in sequence)
        {
            Transform placeHolder = movement.transform.GetChild(0);

            if (!teamObject.GetComponent<PlayerController3D>().restartPosition)
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

        while (teamObject.GetComponent<PlayerController3D>().restartPosition)
        {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1f);
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

    void ReturnToLobby()
    {
        if (isLocalPlayer)
        {
            Debug.Log("ReturnToLobby");
            CmdGoBackToLobby();
        }
    }

    [Command]
    void CmdGoBackToLobby()
    {
        Debug.Log("CmdGoBackToLobby");
        MatchMaker.instance.PlayerDisconnectFromGame(lobbyPlayer, matchID, this.gameObject);
        TargetRpcShowLobby(connectionToClient);
        
    }

    [TargetRpc]
    void TargetRpcShowLobby(NetworkConnection target)
    {
        Debug.Log("TargetRpcShowLobby");
        UILobby.instance.Show();
        UILobby.instance.DisconnectLobby();
    }

}