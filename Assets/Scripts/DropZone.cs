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

        if (Spawner != null)
        {
            d = Spawner.instantiatedChild.GetComponent<Draggable>();
        }
        else
        {
            d = eventData.pointerDrag.GetComponent<Draggable>();
        }
       
        if (d != null) {
            Debug.Log("Draggable = " + d);

            if(int.Parse(d.cardCount.text) > 0 || d.validationParent.GetComponent<DropZone>() != null)
            {
                events.SynchronizeOnDrop(d.index, index);
            }
            
        }
    }

}
