using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Security.Cryptography;


[System.Serializable]
public class Team
{
    public int teamIndex;
    public SyncListGameObject players = new SyncListGameObject();

    public Team(int _teamIndex, GameObject player)
    {
        this.teamIndex = _teamIndex;
        players.Add(player);
    }

    public Team() { }
}


[System.Serializable]
public class SyncListTeam : SyncList<Team> { }

public class TurnManager : NetworkBehaviour
{
    public SyncListTeam teams = new SyncListTeam();

    public void AddNewTeam(int _teamIndex, GameObject _player)
    {
        teams.Add(new Team(_teamIndex, _player));
    }

    public void AddTeammate(int _teamIndex, GameObject _player)
    {
        teams[_teamIndex-1].players.Add(_player);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(mode == LoadSceneMode.Additive)
        {
            for (int i = 0; i < teams.Count; i++)
            {
                Debug.Log(teams[i].teamIndex);
                foreach (var player in teams[i].players)
                {
                    Debug.Log(player);
                }
            }
        }
    }

}
