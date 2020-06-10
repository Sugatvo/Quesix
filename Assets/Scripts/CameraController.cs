using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Cinemachine;


public class CameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

    [SyncVar]
    public GameObject playerObject;

    public override void OnStartAuthority()
    {

        virtualCamera.gameObject.SetActive(true);

        enabled = true;
    }


    void Update()
    {
        if(playerObject != null)
        {
            this.gameObject.transform.position = playerObject.transform.position;
            this.gameObject.transform.rotation = playerObject.transform.rotation;
        }  
    } 
}
