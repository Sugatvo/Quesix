using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SpawnOnDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public int indexPrefab;

    public GameObject instantiatedChild;

    //script of the child to get the drag method
    private Draggable childScript;

    [SerializeField] TextMeshProUGUI cardCount;

    [SerializeField] GameEvents events;

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("OnPointerDown");
        Debug.Log("int.Parse(cardCount.text) = " + int.Parse(cardCount.text));
        Debug.Log("transform.childCount = " + transform.childCount);
        if(int.Parse(cardCount.text) > 0 && transform.childCount == 2)
        {
            Debug.Log("Creando carta y asignando instantiedChild");
            // Crear prefab en los dos integrantes del equipo
            events.CreateCardInstance(indexPrefab);
            instantiatedChild = transform.GetChild(1).gameObject;
          
        }
        else
        {
            Debug.Log("No hacer nada");
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        Debug.Log("OnBeginDrag SpawnOnDrag");
        if (instantiatedChild != null)
        {
            Debug.Log("instantiatedChild != null");
            childScript = instantiatedChild.GetComponent<Draggable>();
            childScript.OnBeginDrag(data);
        }
        else
        {
            Debug.Log("instantiatedChild == null");
        }
    }
    public void OnDrag(PointerEventData data)
    {
        Debug.Log("OnDrag SpawnOnDrag");
        if (instantiatedChild != null)
        {
            childScript.OnDrag(data);
        }
    }
    public void OnEndDrag(PointerEventData data)
    {
        Debug.Log("OnEndDrag SpawnOnDrag");
        if (instantiatedChild != null)
        {
            childScript.OnEndDrag(data);
        }
        instantiatedChild = null;
        childScript = null;
    }

}