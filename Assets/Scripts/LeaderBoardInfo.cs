using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderBoardInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI teamNameText;
    [SerializeField] TextMeshProUGUI timeScoreText;

    private RectTransform _rect = null;
    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return _rect;
        }
    }


    public void SetAttributes(string teamName, string timeScore)
    {
        teamNameText.text = teamName;
        timeScoreText.text = timeScore;
    }
}
