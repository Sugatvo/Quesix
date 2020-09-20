using UnityEngine;
using System.Linq;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Spawner : NetworkBehaviour
{
    public NetworkIdentity cheesePrefab;
    public NetworkIdentity trapPrefab;

    private static List<Transform> spawnPoints = new List<Transform>();

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);
    

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer Spawner");

        for (int i = 0; i < 4; i++)
            SpawnCheese();
        for (int i = 0; i < 2; i++)
            SpawnTrap();
    }

    public void SpawnCheese()
    {
        Debug.Log("SpawnCheese");
        int itemIndex = Random.Range(0, (spawnPoints.Count - 1));
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(itemIndex);
        GameObject newCheese = Instantiate(cheesePrefab.gameObject, spawnPoint.position, spawnPoint.rotation);
        Cheese cheese = newCheese.gameObject.GetComponent<Cheese>();
        cheese.spawner = this;
        NetworkServer.Spawn(newCheese);
        SceneManager.MoveGameObjectToScene(newCheese, gameObject.scene);
    }

    public void SpawnTrap()
    {
        Debug.Log("SpawnTrapt");
        int itemIndex = Random.Range(0, (spawnPoints.Count - 1));
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(itemIndex);
        GameObject newTrap = Instantiate(trapPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);
        Trap trap = newTrap.gameObject.GetComponent<Trap>();
        trap.spawner = this;
        NetworkServer.Spawn(newTrap);
        SceneManager.MoveGameObjectToScene(newTrap, gameObject.scene);
    }
}
