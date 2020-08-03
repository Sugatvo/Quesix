using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;


public class CameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

    [SyncVar]
    public GameObject teamObject;

    [SyncVar]
    public int type = 0;

    [SerializeField] TextMeshProUGUI m_Object;

    public override void OnStartAuthority()
    {
        virtualCamera.gameObject.SetActive(true);
        enabled = true;
        m_Object = GameObject.Find("Rol").GetComponent<TextMeshProUGUI>();
        m_Object.text = $"{TypetoString(type)}";
    }

    public string TypetoString(int type)
    {
        if(type == 1)
        {
            return "Co-piloto";
        }
        else
        {
            return "Piloto";
        }
    }

    void Update()
    {
        if(teamObject != null)
        {
            this.gameObject.transform.position = teamObject.transform.position;
            this.gameObject.transform.rotation = teamObject.transform.rotation;
        }  
    }

}
