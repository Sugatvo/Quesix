using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Autoconnect : MonoBehaviour
{
    [SerializeField]
    NetworkManager networkManager; 

    void Start()
    {
        //Headless build
        if (!Application.isBatchMode) 
        {
            Debug.Log("=== Client connecting ===");

            networkManager.networkAddress = "127.0.0.1";
            networkManager.StartClient();
        }
        else
        {
            Debug.Log("=== Server starting ===");
        }
    }
    public void JoinLocal()
    {
        networkManager.networkAddress = "127.0.0.1";
        networkManager.StartClient();
    }
}
