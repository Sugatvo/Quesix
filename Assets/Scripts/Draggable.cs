using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Canvas canvas;
    [SerializeField] Transform canvasTransform;
    [SerializeField] GameEvents events;

    [SerializeField] public TextMeshProUGUI cardCount;

    public CanvasGroup canvasGroup;
    private RectTransform rectTransform;

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
        rectTransform = GetComponent<RectTransform>();
        canvas = GameObject.Find("ProgrammingCanvas").GetComponent<Canvas>();
        canvasTransform = GameObject.Find("ProgrammingCanvas").GetComponent<Transform>();

        if(type == 0)
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
        // Send to team manager for synchronize

        if (int.Parse(cardCount.text) > 0 || transform.parent.GetComponent<DropZone>() != null)
        {
            events.SynchronizeOnBeginDrag(index);
        }
    }

    public void SyncOnBeginDrag()
    {
        parentToReturnTo = this.transform.parent;
        validationParent = this.transform.parent;
        this.transform.SetParent(canvasTransform);
        droppedOnSlot = false;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        Debug.Log("Card: " + this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Send to team manager for synchronize
        Debug.Log("OnDrag");
        if (int.Parse(cardCount.text) > 0 || validationParent.GetComponent<DropZone>() != null)
        {
            float oldX = canvas.GetComponent<RectTransform>().rect.height * canvas.scaleFactor;
            float oldY = canvas.GetComponent<RectTransform>().rect.width * canvas.scaleFactor;
            events.SynchronizeOnDrag(index, eventData.position, oldX, oldY);
        }
    }

    public void SyncOnDrag(Vector2 mousePosition , float oldX, float oldY)
    {
        if (int.Parse(cardCount.text) > 0 || validationParent.GetComponent<DropZone>() != null)
        {
            float newX = canvas.GetComponent<RectTransform>().rect.height * canvas.scaleFactor;
            float newY = canvas.GetComponent<RectTransform>().rect.width * canvas.scaleFactor;


            float Rx = newX / oldX;
            float Ry = newY / oldY;

            Vector2 realPos = new Vector2(mousePosition.x * Rx, mousePosition.y * Ry);

            this.transform.position = realPos;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (int.Parse(cardCount.text) > 0 || validationParent.GetComponent<DropZone>() != null)
        {
            events.SynchronizeOnEndDrag(index);
        }
    }
}
