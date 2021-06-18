using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeSprite : MonoBehaviour, IPointerClickHandler
{
    private int count = 0;
    [SerializeField] Sprite correct;
    [SerializeField] Sprite warnning;
    [SerializeField] Sprite incorrect;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (count == 0)
        {
            GetComponent<Image>().sprite = correct;
            count += 1;

        }
        else if (count == 1)
        {
            GetComponent<Image>().sprite = warnning;
            count += 1;
        }
        else
        {
            GetComponent<Image>().sprite = incorrect;
            count = 0;
        }
    }
}