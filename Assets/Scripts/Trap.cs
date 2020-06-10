using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Trap : NetworkBehaviour
{
    public bool available = true;
    public Spawner spawner = null;

    // Only call this on server
    [ServerCallback]
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            FallIntoTrap(col.gameObject);
        }
    }

    public void FallIntoTrap(GameObject player)
    {
        if (available)
        {
            // This is a fast switch to prevent two players claiming the prize in a bang-bang close contest for it.
            // First hit turns it off, pending the object being destroyed a few frames later.
            available = false;

            // destroy this one
            NetworkServer.Destroy(gameObject);
        }
    }

}
