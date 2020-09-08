using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;


public class StudentManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerInfo;
    [SerializeField] TextMeshProUGUI rolInfo;

    // Start is called before the first frame update
    void Start()
    {
        if (DBManager.LoggedIn)
        {
            playerInfo.text = "BIENVENIDO - " + DBManager.nombre + " " + DBManager.apellido;
            rolInfo.text = DBManager.rol;
        }
    }

    public void Logout()
    {
        DBManager.LogOut();
        SceneManager.LoadScene(0);
    }

}
