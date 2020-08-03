using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AnswerData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text infoTextObject = null;
    [SerializeField] Image toggle1 = null;
    [SerializeField] Image toggle2 = null;

    [Header("Textures")]
    [SerializeField] Sprite uncheckedToggle = null;
    [SerializeField] Sprite checkedToggle = null;

    [Header("References")]
    [SerializeField] GameEvents events = null;

    private RectTransform _rect = null;
    public RectTransform Rect
    {
        get
        {
            if(_rect == null)
            {
                _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return _rect;
        }
    }

    private int _answerIndex = -1;
    public int AnswerIndex { get { return _answerIndex; } }

    public bool Checked1 = false;
    public bool Checked2 = false;

    public bool SelectionPlayer1 = false;
    public bool SelectionPlayer2 = false;


    public void UpdateData(string info, int index)
    {
        infoTextObject.text = info;
        _answerIndex = index;
    }
    
    public void Reset(bool p1, bool p2)
    {
        if(p1)
        {
            Checked1 = false;
            UpdateUI(p1, p2);
        }
        if(p2)
        {
            Checked2 = false;
            UpdateUI(p1, p2);
        } 
    }

    void UpdateUI(bool p1, bool p2)
    {
        if (p1)
        {
            if (toggle1 == null)
            {
                return;
            }
            toggle1.sprite = (Checked1) ? checkedToggle : uncheckedToggle;

        }
        if (p2)
        {
            if (toggle2 == null)
            {
                return;
            }
            toggle2.sprite = (Checked2) ? checkedToggle : uncheckedToggle;
        }
    }

    public void SwitchState()
    {
        events.SelectAnswer(AnswerIndex);
    }

    public void UpdateAnswersUI()
    {
        // Player 1 select
        if (SelectionPlayer1)
        {
            Checked1 = !Checked1;
        }

        // Player 2 select
        if (SelectionPlayer2)
        {
            Checked2 = !Checked2;
        }

        UpdateUI(SelectionPlayer1, SelectionPlayer2);
    }
}
