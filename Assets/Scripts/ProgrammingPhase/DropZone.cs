using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MovCardData currentMovement = null;

    [SerializeField] public TextMeshProUGUI movementAction;

    [SerializeField] GameEvents events;


    public int index;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        SpawnOnDrag Spawner = eventData.pointerDrag.GetComponent<SpawnOnDrag>();
        Draggable d = null;

        if (Spawner != null && Spawner.instantiatedChild != null)
        {
            d = Spawner.instantiatedChild.GetComponent<Draggable>();
            Debug.Log("Draggable from Spawner = " + d);
        }
        // Verificar si la eventData contiene un Draggable 
        else if (eventData.pointerDrag.GetComponent<Draggable>() != null)
        {
            d = eventData.pointerDrag.GetComponent<Draggable>();
            Debug.Log("Draggable from eventData = " + d);
        }

        if(d != null)
        {
            if (int.Parse(d.cardCount.text) > 0 || d.validationParent.GetComponent<DropZone>() != null)
            {
                events.SynchronizeOnDrop(d.index, index);
            }
        }
    }

}
