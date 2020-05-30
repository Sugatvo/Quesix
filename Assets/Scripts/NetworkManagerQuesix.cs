using UnityEngine;

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
        public Transform leftCheeseSpawn;
        public Transform rightCheeseSpawn;
        GameObject cheese;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);

            // spawn ball if two players
            if (numPlayers == 2)
            {
                for(int i = 0; i < 2; i++)
                {
                    if(i == 0)
                        cheese = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Queso"), leftCheeseSpawn.position, start.rotation);
                    else
                        cheese = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Queso"), rightCheeseSpawn.position, start.rotation);
                    NetworkServer.Spawn(cheese);
                }
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // destroy ball
            if (cheese != null)
                NetworkServer.Destroy(cheese);

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}