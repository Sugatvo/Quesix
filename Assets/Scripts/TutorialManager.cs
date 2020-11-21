using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public Animator m_Animator;
    public Animator p_Animator;

    private bool m_pilot = false;
    private bool m_copilot = false; 

    public bool isTutorial = false;

    private static TutorialManager _instance;
    public static TutorialManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        m_pilot = m_Animator.GetBool("isPilot");
        m_copilot = m_Animator.GetBool("isCopilot");
        isTutorial = m_Animator.GetBool("isTutorial");
    }



}
