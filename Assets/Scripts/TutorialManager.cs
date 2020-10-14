using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex;

    public Animator m_Animator;

    public float waitTime;

    private bool m_pilot = false;
    private bool m_copilot = false;

    private void Start()
    {
        popUpIndex = m_Animator.GetInteger("PopUpIndex");
        Debug.Log("popUpIndex: " + popUpIndex);
        waitTime = 5.0f;
        Debug.Log("waitTime: " + waitTime);
    }

    void Update()
    {
        if(popUpIndex == 0)
        {
            if (waitTime <= 0)
            {
                waitTime = 5.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (popUpIndex == 1)
        {
            if (waitTime <= 0)
            { 
                waitTime = 1.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (popUpIndex == 2)
        {
            if(waitTime <= 0)
            {
                // Tiempo de espera de la descripcion de los quesos recolectados
                waitTime = 5.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
            
        }
        if (popUpIndex == 3)
        {
            if (waitTime <= 0)
            {
                waitTime = 1.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }

        }
        if (popUpIndex == 4)
        {
            if (waitTime <= 0)
            {
                // Tiempo de espera de la descripcion de los digipasos obtenidos
                waitTime = 5.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }

        }
        if (popUpIndex == 5)
        {
            if (waitTime <= 0)
            {
                waitTime = 1.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }

        }

        if (popUpIndex == 6)
        {
            if (waitTime <= 0)
            {
                waitTime = 10.0f;
                popUpIndex++;
                m_Animator.SetInteger("PopUpIndex", popUpIndex);
                Debug.Log("popUpIndex: " + popUpIndex);
                m_pilot = m_Animator.GetBool("isPilot");
                m_copilot = m_Animator.GetBool("isCopilot");

            }
            else
            {
                waitTime -= Time.deltaTime;
            }

        }

        if (m_pilot)
        {
            if (waitTime <= 0)
            {
                m_pilot = false;
                m_Animator.SetBool("isPilot", m_pilot);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (m_copilot)
        {
            if (waitTime <= 0)
            {
                m_copilot = false;
                m_Animator.SetBool("isCopilot", m_copilot);
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

    }
}
