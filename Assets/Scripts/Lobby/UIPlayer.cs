using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIPlayer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI playerName;

    NetworkPlayer player;

    public void SetPlayer(NetworkPlayer player)
    {
        this.player = player;
        playerName.text = player.nombre + " " + player.apellido;
    }
}
