using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovCardData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text titleText = null;
    public Text TitleText { get { return titleText; } }
    [SerializeField] Image moveImg = null;


    public void UpdateData(string titulo, Sprite imgSprite)
    {
        titleText.text = titulo;
        moveImg.sprite = imgSprite;

    }
}
