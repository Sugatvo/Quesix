using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CheeseSpawnPoint : NetworkBehaviour
{
    private void Awake() => Spawner.AddSpawnPoint(transform);
    private void OnDestroy() => Spawner.RemoveSpawnPoint(transform);

}