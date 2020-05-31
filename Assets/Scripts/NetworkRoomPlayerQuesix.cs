using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;

public class NetworkRoomPlayerQuesix : NetworkRoomPlayer
{
    public override void OnStartClient()
    {
        if (LogFilter.Debug) Debug.LogFormat("OnStartClient {0}", SceneManager.GetActiveScene().path);

        base.OnStartClient();
    }

    public override void OnClientEnterRoom()
    {
        if (LogFilter.Debug) Debug.LogFormat("OnClientEnterRoom {0}", SceneManager.GetActiveScene().path);
    }

    public override void OnClientExitRoom()
    {
        if (LogFilter.Debug) Debug.LogFormat("OnClientExitRoom {0}", SceneManager.GetActiveScene().path);
    }
}
