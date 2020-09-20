using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ElementSpawnPoint : NetworkBehaviour
{
    private void Awake()
    {
        Spawner.AddSpawnPoint(transform);
    }
    private void OnDestroy() => Spawner.RemoveSpawnPoint(transform);

}