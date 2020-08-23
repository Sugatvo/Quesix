using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeColor : MonoBehaviour, IPointerClickHandler
{
    private int count = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(count == 0)
        {
            GetComponent<Image>().color = Color.green;
            count += 1;

        }
        else if(count == 1)
        {
            GetComponent<Image>().color = Color.yellow;
            count += 1;
        }
        else
        {
            GetComponent<Image>().color = Color.red;
            count = 0;
        }
    }
}
