using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControllerTutorial : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineStateDrivenCamera StateDrivenCamera = null;

    [SerializeField] GameEvents events;

    float offset;
    public bool isMoving = false;
    public GameObject teamObject;
    public Animator m_animator;

    int previous_State;

    private static CameraControllerTutorial _instance;
    public static CameraControllerTutorial Instance { get { return _instance; } }

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

    void Start()
    {
        m_animator = StateDrivenCamera.GetComponent<Animator>();
        // Left
        m_animator.SetBool("backToLeft", true);
        offset = 90f;
    }

    void Update()
    {
        if (isMoving)
        {
            this.gameObject.transform.position = teamObject.transform.position;

            Quaternion rotation = Quaternion.Euler(0, teamObject.transform.rotation.eulerAngles.y + offset, 0);

            this.gameObject.transform.rotation = rotation;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                m_animator.SetBool("isQ", true);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                m_animator.SetBool("isE", true);
            }
        }
    }


    public void Move(bool state)
    {
        Debug.Log("Move()");
        Debug.Log("State: " + state);
        if (state)
        {
            previous_State = m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        }
        else
        {
            if (previous_State == Animator.StringToHash("Left"))
            {
                m_animator.SetBool("backToLeft", true);
            }

            if (previous_State == Animator.StringToHash("Right"))
            {
                m_animator.SetBool("backToRight", true);
            }

            if (previous_State == Animator.StringToHash("Up"))
            {
                m_animator.SetBool("backToUp", true);
            }

            if (previous_State == Animator.StringToHash("Bottom"))
            {
                m_animator.SetBool("backToBottom", true);
            }
        }
        isMoving = state;
        m_animator.SetBool("isMoving", state);
    }
}
