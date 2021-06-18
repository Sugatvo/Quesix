using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToggleData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI toggleInfo = null;

    public void UpdateData(string _toggleInfo)
    {
        toggleInfo.text = _toggleInfo;
    }
}
