using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerRoomQuesix : NetworkRoomManager
{
    public List<int> team_test = new List<int>();

    private int group_index = 1;

    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        PlayerScoreQuesix playerScore = gamePlayer.GetComponent<PlayerScoreQuesix>();
        playerScore.id_team = roomPlayer.GetComponent<NetworkRoomPlayerQuesix>().id_team;

        foreach (int i in team_test)
        {
            Item item = new Item
            {
                name = "Jugador",
                player_id = i
            };
            playerScore.equipo.Add(item);
        }
        return true;
    }

    public override void OnRoomStopClient()
    {
        // Demonstrates how to get the Network Manager out of DontDestroyOnLoad when
        // going to the offline scene to avoid collision with the one that lives there.
        if (gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        // Demonstrates how to get the Network Manager out of DontDestroyOnLoad when
        // going to the offline scene to avoid collision with the one that lives there.
        if (gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

        base.OnRoomStopServer();
    }

    /*
        This code below is to demonstrate how to do a Start button that only appears for the Host player
        showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
        all players are ready, but if a player cancels their ready state there's no callback to set it back to false
        Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
        Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
        is set as DontDestroyOnLoad = true.
    */

    bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
        if (isHeadless)
            base.OnRoomServerPlayersReady();
        else
            showStartButton = true;
            
    }


    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;
            ServerChangeScene(GameplayScene);
        }
    }


    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnection conn)
    {
        if (LogFilter.Debug) Debug.Log("NetworkRoomManager OnServerReady");

        if (conn.identity == null)
        {
            // this is now allowed (was not for a while)
            if (LogFilter.Debug) Debug.Log("Ready with no player object");
        }
        NetworkServer.SetClientReady(conn);

        if (conn != null && conn.identity != null)
        {
            GameObject roomPlayer = conn.identity.gameObject;

            // if null or not a room player, dont replace it
            if (roomPlayer != null && roomPlayer.GetComponent<NetworkRoomPlayerQuesix>() != null)
            {
                if (roomPlayer.GetComponent<NetworkRoomPlayerQuesix>().id_team == 0)
                {
                    roomPlayer.GetComponent<NetworkRoomPlayerQuesix>().id_team = group_index;
                    team_test.Add(roomPlayer.GetComponent<NetworkRoomPlayerQuesix>().index);
                }
                if (team_test.Count == 2)
                {
                    SceneLoadedForPlayer(conn, roomPlayer);
                    team_test = new List<int>();
                    group_index += 1;
                }
            }

            
        }
    }

    void SceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        if (LogFilter.Debug) Debug.LogFormat("NetworkRoom SceneLoadedForPlayer scene: {0} {1}", SceneManager.GetActiveScene().path, conn);

        if (IsSceneActive(RoomScene))
        {
            // cant be ready in room, add to ready list
            PendingPlayer pending;
            pending.conn = conn;
            pending.roomPlayer = roomPlayer;
            pendingPlayers.Add(pending);
            return;
        }

        GameObject gamePlayer = OnRoomServerCreateGamePlayer(conn, roomPlayer);
        if (gamePlayer == null)
        {
            // get start position from base class
            Transform startPos = GetStartPosition();
            gamePlayer = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (!OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer))
            return;

        // replace room player with game player
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);
    }

}

