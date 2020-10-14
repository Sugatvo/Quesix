using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovCardData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] string cardAction = null;
    public string CardAction { get { return cardAction; } }
    [SerializeField] Image moveImg = null;


    public void UpdateData(string titulo, Sprite imgSprite)
    {
        cardAction = titulo;
        moveImg.sprite = imgSprite;

    }
}
