using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkQuesixManager : NetworkManager
{
    [Header("MultiScene Setup")]
    public int instances = 3;

    [Scene]
    public string gameScene;

    readonly List<Scene> subScenes = new List<Scene>();




}
