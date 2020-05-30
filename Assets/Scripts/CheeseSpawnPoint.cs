using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CheeseSpawnPoint : MonoBehaviour
{
    private void Awake() => NetworkManagerQuesix.AddSpawnPoint(transform);
    private void OnDestroy() => NetworkManagerQuesix.RemoveSpawnPoint(transform);

}