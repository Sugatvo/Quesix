using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GlobalTimer : NetworkBehaviour
{
    public int MaxTime;
    public bool isReady = false;
    private float waitTime = 46f;
    private TextMeshProUGUI globalTimerText;

    public override void OnStartLocalPlayer()
    {
        globalTimerText = GameObject.Find("GlobalTimerText").GetComponent<TextMeshProUGUI>();
    }

    public void SetMaxTime(int time)
    {
        string auxMinutes;
        string auxSeconds;

        MaxTime = time;

        var minutes = Mathf.Floor(time / 60);

        if (minutes < 10)
        {
            auxMinutes = "0" + minutes.ToString();
        }
        else
        {
            auxMinutes = minutes.ToString();
        }
        var seconds = time - minutes * 60;

        if (seconds < 10)
        {
            auxSeconds = "0" + seconds.ToString();
        }
        else
        {
            auxSeconds = seconds.ToString();
        }

        globalTimerText.text = auxMinutes + ":" + auxSeconds;
    }

    private void Update()
    {
        if(waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else
        {
            isReady = true;
        }
    }
}
