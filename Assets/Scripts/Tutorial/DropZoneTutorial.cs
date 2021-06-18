using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropZoneTutorial : MonoBehaviour, IDropHandler
{
    public MovCardData currentMovement = null;

    [SerializeField] public TextMeshProUGUI movementAction;

    public int index;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        SpawnOnDragTutorial Spawner = eventData.pointerDrag.GetComponent<SpawnOnDragTutorial>();
        DraggableTutorial d = null;

        if (Spawner != null && Spawner.instantiatedChild != null)
        {
            d = Spawner.instantiatedChild.GetComponent<DraggableTutorial>();
            Debug.Log("Draggable from Spawner = " + d);
        }
        // Verificar si la eventData contiene un Draggable 
        else if (eventData.pointerDrag.GetComponent<DraggableTutorial>() != null)
        {
            d = eventData.pointerDrag.GetComponent<DraggableTutorial>();
            Debug.Log("Draggable from eventData = " + d);
        }

        if (d != null)
        {
            if (ManagerPiloto.Instance.isFirstDrop)
            {
                ManagerPiloto.Instance.GoToRevisar1Tutorial();
                ManagerPiloto.Instance.isFirstDrop = false;
            }
            // Si el dropzone ya tiene un movimiento, removerlo y asignar el nuevo
            if (currentMovement != null)
            {
                // Restar -1 al contador de la carta correspondiente
                if (currentMovement.CardAction.Equals("Izquierda"))
                {
                    ManagerPiloto.Instance.leftCountText.text = (int.Parse(ManagerPiloto.Instance.leftCountText.text) + 1).ToString();
                }
                else if (currentMovement.CardAction.Equals("Avanzar"))
                {
                    ManagerPiloto.Instance.fowardCountText.text = (int.Parse(ManagerPiloto.Instance.fowardCountText.text) + 1).ToString();
                }
                else if (currentMovement.CardAction.Equals("Retroceder"))
                {
                    ManagerPiloto.Instance.backCountText.text = (int.Parse(ManagerPiloto.Instance.backCountText.text) + 1).ToString();
                }
                else
                {
                    ManagerPiloto.Instance.rightCountText.text = (int.Parse(ManagerPiloto.Instance.rightCountText.text) + 1).ToString();
                }
                ManagerPiloto.Instance.cardCountText.text = (int.Parse(ManagerPiloto.Instance.cardCountText.text) + 1).ToString();
                Destroy(currentMovement.gameObject);
            }
            d.parentToReturnTo = transform;
            d.droppedOnSlot = true;
            currentMovement = d.gameObject.GetComponent<MovCardData>();

            if (currentMovement.CardAction.Equals("Izquierda"))
            {
                movementAction.text = "Girar";
            }
            else if (currentMovement.CardAction.Equals("Avanzar"))
            {
                movementAction.text = "Avanzar";
            }
            else if (currentMovement.CardAction.Equals("Retroceder"))
            {
                movementAction.text = "Retroceder";
            }
            else
            {
                movementAction.text = "Girar";
            }
        }
    }

}
