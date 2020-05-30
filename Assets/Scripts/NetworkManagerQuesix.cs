using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Mirror
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class NetworkManagerQuesix : NetworkManager
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
    
        private static List<Transform> spawnPoints = new List<Transform>();

        public static void AddSpawnPoint(Transform transform)
        {
            spawnPoints.Add(transform);
            spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
        }
        public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);


        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);

            // spawn ball if two players
            if (numPlayers == 2)
            {
                // Agregar Trampas
                for (int i = 0; i < 2; i++)
                {
                    int itemIndex = Random.Range(0, (spawnPoints.Count - 1));
                    Transform spawnPoint = spawnPoints.ElementAtOrDefault(itemIndex);

                    if (spawnPoint == null)
                    {
                        Debug.LogError($"Missing spawn point for trap {i}");
                        return;
                    }
                    GameObject cheeseInstance = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Trampa"), spawnPoints[itemIndex].position, spawnPoints[itemIndex].rotation);
                    NetworkServer.Spawn(cheeseInstance, conn);
                    RemoveSpawnPoint(spawnPoint);

                }

                // Agregar Quesos
                for (int i = 0; i < 4; i++)
                {
                    int itemIndex = Random.Range(0, (spawnPoints.Count - 1));
                    Transform spawnPoint = spawnPoints.ElementAtOrDefault(itemIndex);

                    if (spawnPoint == null)
                    {
                        Debug.LogError($"Missing spawn point for cheese {i}");
                        return;
                    }
                    GameObject cheeseInstance = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Queso"), spawnPoints[itemIndex].position, spawnPoints[itemIndex].rotation);
                    NetworkServer.Spawn(cheeseInstance, conn);

                }
            }
        }


        public override void OnServerDisconnect(NetworkConnection conn)
        {

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}