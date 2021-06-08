using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;

public class CameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineStateDrivenCamera StateDrivenCamera = null;

    [SerializeField] GameEvents events;

    [SyncVar]
    public GameObject teamObject;

    [SyncVar]
    public string rol = string.Empty;

    float offset;

    public bool isMoving = false;

    public Animator m_animator;

    int previous_State;

    public override void OnStartLocalPlayer()
    {
        StateDrivenCamera.gameObject.SetActive(true);
        m_animator = StateDrivenCamera.GetComponent<Animator>();

        foreach (CinemachineVirtualCamera virtualCamera in StateDrivenCamera.ChildCameras)
        {
            if (virtualCamera.name.Equals("Left"))
            {
                virtualCamera.LookAt = GameObject.Find("LeftCenter").transform;
                virtualCamera.Follow = GameObject.Find("LeftCenter").transform;
            }
            else if (virtualCamera.name.Equals("Right"))
            {
                virtualCamera.LookAt = GameObject.Find("RightCenter").transform;
                virtualCamera.Follow = GameObject.Find("RightCenter").transform;
            }
            else if (virtualCamera.name.Equals("Up"))
            {
                virtualCamera.LookAt = GameObject.Find("UpCenter").transform;
                virtualCamera.Follow = GameObject.Find("UpCenter").transform;
            }
            else if(virtualCamera.name.Equals("Bottom"))
            {
                virtualCamera.LookAt = GameObject.Find("BottomCenter").transform;
                virtualCamera.Follow = GameObject.Find("BottomCenter").transform;
            }
        }

        if (rol.Equals("Profesor"))
        {
            m_animator.SetBool("backToLeft", true);
            UIManager.Instance.HideForTeacher();
        }
        else
        {
            events.PlayerIsMoving += Move;

            if (teamObject.transform.position.z == 9)
            {
                Debug.Log("teamObject is on left");
                // Left
                m_animator.SetBool("backToLeft", true);
            }
            else if (teamObject.transform.position.z == -9)
            {
                Debug.Log("teamObject is on Right");
                // Right
                m_animator.SetBool("backToRight", true);
            }
            else if (teamObject.transform.position.x == -9)
            {
                Debug.Log("teamObject is on Bot");
                // Bot
                m_animator.SetBool("backToBottom", true);
            }
            else
            {
                Debug.Log("teamObject is on Top");
                // Top
                m_animator.SetBool("backToUp", true);
            }
        }

        enabled = true;
        offset = 90f;
    }


    void OnDisable()
    {
        events.PlayerIsMoving -= Move;
    }

    void Update()
    { 
        if(teamObject != null && isMoving )
        {
            this.gameObject.transform.position = teamObject.transform.position;

            Quaternion rotation = Quaternion.Euler(0, teamObject.transform.rotation.eulerAngles.y + offset, 0);

            this.gameObject.transform.rotation = rotation;
        }
        if (isLocalPlayer)
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
