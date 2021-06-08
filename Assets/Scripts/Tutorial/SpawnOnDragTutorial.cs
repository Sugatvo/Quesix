using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SpawnOnDragTutorial : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public int indexPrefab;
    public GameObject instantiatedChild;

    //script of the child to get the drag method
    private DraggableTutorial childScript;

    [SerializeField] TextMeshProUGUI cardCount;

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("OnPointerDown");
        Debug.Log("int.Parse(cardCount.text) = " + int.Parse(cardCount.text));
        Debug.Log("transform.childCount = " + transform.childCount);
        if (int.Parse(cardCount.text) > 0 && transform.childCount == 2)
        {
            Debug.Log("Creando carta y asignando instantiedChild");
            // Crear prefab en los dos integrantes del equipo
            if (indexPrefab == 0)
            {
                ManagerPiloto.Instance.currentCardInMovement = Instantiate(ManagerPiloto.Instance.leftPrefab, ManagerPiloto.Instance.leftSpawner, false);
            }
            else if (indexPrefab == 1)
            {
                ManagerPiloto.Instance.currentCardInMovement = Instantiate(ManagerPiloto.Instance.fowardPrefab, ManagerPiloto.Instance.fowardSpawner, false);
            }
            else if (indexPrefab == 2)
            {
                ManagerPiloto.Instance.currentCardInMovement = Instantiate(ManagerPiloto.Instance.backPrefab, ManagerPiloto.Instance.backSpawner, false);
            }
            else
            {
                ManagerPiloto.Instance.currentCardInMovement = Instantiate(ManagerPiloto.Instance.rightPrefab, ManagerPiloto.Instance.rightSpawner, false);
            }
            ManagerPiloto.Instance.Cards.Add(ManagerPiloto.Instance.currentCardInMovement);
            ManagerPiloto.Instance.currentCardInMovement.GetComponent<DraggableTutorial>().index = ManagerPiloto.Instance.Cards.IndexOf(ManagerPiloto.Instance.currentCardInMovement);
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
            childScript = instantiatedChild.GetComponent<DraggableTutorial>();
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