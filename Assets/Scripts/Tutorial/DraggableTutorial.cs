using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableTutorial : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Transform canvasTransform;
    [SerializeField] public TextMeshProUGUI cardCount;

    public CanvasGroup canvasGroup;

    public bool droppedOnSlot;

    public Transform parentToReturnTo = null;
    public Transform validationParent = null;

    Vector3 startPosition;

    public int index;
    public int type;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = GetComponent<RectTransform>().anchoredPosition;
        canvasTransform = GameObject.Find("ProgrammingCanvas").GetComponent<Transform>();

        if (type == 0)
        {
            cardCount = GameObject.Find("LeftCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        }
        else if (type == 1)
        {
            cardCount = GameObject.Find("FowardCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        }
        else if (type == 2)
        {
            cardCount = GameObject.Find("BackCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            cardCount = GameObject.Find("RightCount").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    // Transmitir eventos a los dos jugadores

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag Draggable");

        if (ManagerPiloto.Instance.isFirstDrag)
        {
            ManagerPiloto.Instance.GoToArrastrarMovimiento2Tutorial();
            ManagerPiloto.Instance.isFirstDrag = false;
        }

        if (int.Parse(cardCount.text) > 0 || transform.parent.GetComponent<DropZoneTutorial>() != null)
        {
            parentToReturnTo = this.transform.parent;
            validationParent = this.transform.parent;
            this.transform.SetParent(canvasTransform);
            droppedOnSlot = false;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            Debug.Log("Card: " + this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag Draggable");
        // Send to team manager for synchronize
        if (validationParent != null)
        {
            if (int.Parse(cardCount.text) > 0 || validationParent.GetComponent<DropZoneTutorial>() != null)
            {
                this.transform.position = eventData.position;
            }
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag Draggable");
        if (int.Parse(cardCount.text) > 0 || validationParent.GetComponent<DropZoneTutorial>() != null)
        {
            Debug.Log("Card: " + this);
            Debug.Log("Validation droppedOnSlot = " + droppedOnSlot);
            // Si la carta fue soltada dentor de un dropzone
            if (droppedOnSlot)
            {
                // Verificar de donde viene y ver si quitar o no movimientos
                SpawnOnDragTutorial Spawner = validationParent.GetComponent<SpawnOnDragTutorial>();

                if (Spawner != null)
                {
                    Debug.Log("Spawner drop on slot");

                    // Restar -1 al texto de la carta correspondiente
                    MovCardData cardInfo = GetComponent<MovCardData>();
                    if (cardInfo.CardAction.Equals("Izquierda"))
                    {
                        ManagerPiloto.Instance.leftCountText.text = (int.Parse(ManagerPiloto.Instance.leftCountText.text) - 1).ToString();
                    }
                    else if (cardInfo.CardAction.Equals("Avanzar"))
                    {
                        ManagerPiloto.Instance.fowardCountText.text = (int.Parse(ManagerPiloto.Instance.fowardCountText.text) - 1).ToString();
                    }
                    else if (cardInfo.CardAction.Equals("Retroceder"))
                    {
                        ManagerPiloto.Instance.backCountText.text = (int.Parse(ManagerPiloto.Instance.backCountText.text) - 1).ToString();
                    }
                    else
                    {
                        ManagerPiloto.Instance.rightCountText.text = (int.Parse(ManagerPiloto.Instance.rightCountText.text) - 1).ToString();
                    }
                    ManagerPiloto.Instance.cardCountText.text = (int.Parse(ManagerPiloto.Instance.cardCountText.text) - 1).ToString();
                }
                else
                {
                    // Viene de dropzone, por lo tanto no se resta al contador
                    Debug.Log("Dropzone drop on slot");
                    DropZoneTutorial dropZone = validationParent.GetComponent<DropZoneTutorial>();

                    // Borrar texto del dropzone anterior
                    if (dropZone != null)
                    {
                        dropZone.movementAction.text = string.Empty;
                    }
                    dropZone.currentMovement = null;
                }

                transform.SetParent(parentToReturnTo);
                GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1.0f;

                // Remover cartas destruidas
                ManagerPiloto.Instance.Cards.RemoveAll(x => x == null);

                // Actualizar index de las cartas
                foreach (var item in ManagerPiloto.Instance.Cards)
                {
                    item.GetComponent<DraggableTutorial>().index = ManagerPiloto.Instance.Cards.IndexOf(item);
                }

            }
            // Si la carta fue soltada fuera de un dropzone
            else
            {
                SpawnOnDragTutorial Spawner = validationParent.GetComponent<SpawnOnDragTutorial>();

                if (Spawner != null)
                {
                    Debug.Log("Spawner dont drop on slot");
                }
                else
                {
                    Debug.Log("Dropzone dont drop on slot");
                    DropZoneTutorial dropZone = validationParent.GetComponent<DropZoneTutorial>();
                    if (dropZone != null)
                    {
                        dropZone.movementAction.text = string.Empty;
                    }

                    // Agregar +1 al texto de la carta correspondiente
                    MovCardData cardInfo = GetComponent<MovCardData>();
                    if (cardInfo.CardAction.Equals("Izquierda"))
                    {
                        ManagerPiloto.Instance.leftCountText.text = (int.Parse(ManagerPiloto.Instance.leftCountText.text) + 1).ToString();
                    }
                    else if (cardInfo.CardAction.Equals("Avanzar"))
                    {
                        ManagerPiloto.Instance.fowardCountText.text = (int.Parse(ManagerPiloto.Instance.fowardCountText.text) + 1).ToString();
                    }
                    else if (cardInfo.CardAction.Equals("Retroceder"))
                    {
                        ManagerPiloto.Instance.backCountText.text = (int.Parse(ManagerPiloto.Instance.backCountText.text) + 1).ToString();
                    }
                    else
                    {
                        ManagerPiloto.Instance.rightCountText.text = (int.Parse(ManagerPiloto.Instance.rightCountText.text) + 1).ToString();
                    }
                    ManagerPiloto.Instance.cardCountText.text = (int.Parse(ManagerPiloto.Instance.cardCountText.text) + 1).ToString();
                }

                Destroy(gameObject);

                // Remover cartas destruidas y actualizar indices
                ManagerPiloto.Instance.Cards.RemoveAll(x => x == null);
                foreach (var item in ManagerPiloto.Instance.Cards)
                {
                    item.GetComponent<DraggableTutorial>().index = ManagerPiloto.Instance.Cards.IndexOf(item);
                }
            }

        }
    }
}
