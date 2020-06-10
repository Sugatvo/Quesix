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

    [SyncVar]
    public Vector2 start_pos;

    public SyncListItem equipo = new SyncListItem();

    public List<NetworkConnection> team_connections;

    private NetworkIdentity objNetId;

    private GUIStyle white;
    private GUIStyle grey;
    private GUIStyle black;


    void OnEnable()
    {
        white = new GUIStyle();
        white.normal.textColor = Color.white;

        grey = new GUIStyle();
        grey.normal.textColor = Color.grey;

        black = new GUIStyle();
        black.normal.textColor = Color.black;
    }

    /*
    [ServerCallback]
    void CmdAssignCameraTracker()
    {
        foreach(NetworkConnection conn in team_connections)
        {
            Debug.Log(conn);
            Debug.Log(conn.identity);
            Debug.Log(conn.identity.gameObject);
            conn.identity.gameObject.GetComponent<CameraController>().playerObject = this.gameObject;
        }
    }

    void Start()
    {
        CmdAssignCameraTracker();
    }*/

    void Update()
    {
        if (this.gameObject.GetComponent<NetworkIdentity>().hasAuthority && Input.GetKeyDown(KeyCode.Space))
        {
            CmdCambiarAutoridad();
        }
        
    }


    [Command]
    void CmdCambiarAutoridad()
    {

        // Lista solo accesible por servidor
        if (team_connections[0] == connectionToClient)
        {
            objNetId = this.gameObject.GetComponent<NetworkIdentity>();
            objNetId.RemoveClientAuthority();
            objNetId.AssignClientAuthority(team_connections[1]);
        }
        else
        {
            objNetId = this.gameObject.GetComponent<NetworkIdentity>();
            objNetId.RemoveClientAuthority();
            objNetId.AssignClientAuthority(team_connections[0]);
        }

    }

    public void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200f + (id_team * 100), 10f, 90f, 130f));
        GUILayout.Label($"Equipo {id_team}: {score.ToString("00")}", black);
        foreach (Item i in equipo)
        {
            GUILayout.Label($"Jugador {i.player_id}", black);
        }
        GUILayout.EndArea();
    }

}
