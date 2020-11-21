using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GlobalTimer : NetworkBehaviour
{
    public int MaxTime;
    public int CurrentTime;
    public bool isReady = false;
    public bool isTutorial = false;
    private float waitTime = 46f;
    private TextMeshProUGUI globalTimerText;


    public override void OnStartLocalPlayer()
    {
        globalTimerText = GameObject.Find("GlobalTimerText").GetComponent<TextMeshProUGUI>();
    }

    public void SetTime(int time)
    {
        string auxMinutes;
        string auxSeconds;

        CurrentTime = time;

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

    public void SetTutorial(bool statement)
    {
        TutorialManager.Instance.m_Animator.SetBool("isTutorial", statement);
        GetComponent<TeamManager>().isFirstQuestion = statement;
        GetComponent<TeamManager>().isFirstProgramming = statement;
    }

    private void Update()
    {
        if (isTutorial)
        {
            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
            }
            else
            {
                isReady = true;
            }

        }
        else
        {
            isReady = true;
        }
       
    }
}
