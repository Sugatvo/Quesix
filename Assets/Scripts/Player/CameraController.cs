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

    public override void OnStartAuthority()
    {
        virtualCamera.gameObject.SetActive(true);
        enabled = true;
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
