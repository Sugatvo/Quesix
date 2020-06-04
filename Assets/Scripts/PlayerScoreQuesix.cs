using Mirror;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Item
{
    public string name;
    public int player_id;
}

[System.Serializable]
public class SyncListItem : SyncList<Item> { }

public class PlayerScoreQuesix : NetworkBehaviour
{
    [SyncVar]
    public uint score;

    [SyncVar]
    public int id_team;

    public SyncListItem equipo = new SyncListItem();

    public void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200f + (id_team * 100), 10f, 90f, 130f));
        GUILayout.Label($"Equipo {id_team}");
        foreach (Item i in equipo)
        {
            GUILayout.Label($"Jugador {i.player_id}");
        }
        GUILayout.EndArea();
    }


}
