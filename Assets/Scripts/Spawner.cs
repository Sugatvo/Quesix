using UnityEngine;
using System.Linq;
using Mirror;
using System.Collections.Generic;

public class Spawner : NetworkBehaviour
{
    public NetworkIdentity cheesePrefab;

    private static List<Transform> spawnPoints = new List<Transform>();


    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    public override void OnStartServer()
    {
        for (int i = 0; i < 4; i++)
            SpawnCheese();
    }

    public void SpawnCheese()
    {
        int itemIndex = Random.Range(0, (spawnPoints.Count - 1));
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(itemIndex);
        GameObject newCheese = Instantiate(cheesePrefab.gameObject, spawnPoints[itemIndex].position, spawnPoints[itemIndex].rotation);
        Cheese cheese = newCheese.gameObject.GetComponent<Cheese>();
        cheese.spawner = this;
        NetworkServer.Spawn(newCheese);
    }
}
