using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITeam : MonoBehaviour
{
    [Header("Player 1 Information")]
    public int player1_id_alumno;
    [SerializeField] TextMeshProUGUI player1_Name;
    [SerializeField] TextMeshProUGUI player1_Status;
    [SerializeField] Image player1_Status_Icon;

    [Header("Player 2 Information")]
    public int player2_id_alumno;
    [SerializeField] TextMeshProUGUI player2_Name;
    [SerializeField] TextMeshProUGUI player2_Status;
    [SerializeField] Image player2_Status_Icon;

    [Header("Team Information")]
    private int id_equipo;
    [SerializeField] TextMeshProUGUI teamName;
    private Color ratonIconColor;
    [SerializeField] Image ratonIcon;
    [SerializeField] Image borderTeamUI;


    private Color connected = new Color(0.1490196f, 0.7529413f, 0.6705883f, 1f);
    private Color disconnected = new Color(0.5882353f, 0.5882353f, 0.5882353f, 1f);

    public void SetTeamName(string _name)
    {
        teamName.text = _name;
    }

    public void SetID(int id)
    {
        id_equipo = id;
    }

    public void SetPlayer1(int _id, string nombre, string apellido)
    {
        player1_id_alumno = _id;
        player1_Name.text = nombre + " " + apellido;
    }
    public void SetPlayer2(int _id, string nombre, string apellido)
    {
        player2_id_alumno = _id;
        player2_Name.text = nombre + " " + apellido;
    }

    public void SetPlayerStatus(NetworkPlayer player)
    {
        if (player1_Name.text.Equals(player.nombre + " " + player.apellido))
        {
            player1_Status.text = "Conectado";
            player1_Status.color = connected;
            player1_Status_Icon.color = connected;
        }

        if (player2_Name.text.Equals(player.nombre + " " + player.apellido))
        {
            player2_Status.text = "Conectado";
            player2_Status.color = connected;
            player2_Status_Icon.color = connected;
        }
    }

    public void SetTeamColor(Color _color)
    {
       ratonIcon.color = _color;
    }

    public bool isMember(NetworkPlayer _player)
    {
        if(int.Parse(_player.id_student) == player1_id_alumno || int.Parse(_player.id_student) == player2_id_alumno)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
