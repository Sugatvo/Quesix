using UnityEngine;
using Mirror;

public class Cheese : NetworkBehaviour
{
    public bool available = true;
    public Spawner spawner = null;
    int points;

    // Only call this on server
    [ServerCallback]
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            ClaimPrize(col.gameObject);
        }
    }

    public void ClaimPrize(GameObject player)
    {
        if (available)
        {
            
            // This is a fast switch to prevent two players claiming the prize in a bang-bang close contest for it.
            // First hit turns it off, pending the object being destroyed a few frames later.
            available = false;

            // calculate the points from the color ... lighter scores higher as the average approaches 255
            // UnityEngine.Color RGB values are float fractions of 255
            points = 1;
            if (LogFilter.Debug) Debug.LogFormat("Scored {0} points", points);

            // award the points via SyncVar on the PlayerController
            player.transform.parent.GetComponent<PlayerScoreQuesix>().AddScore(points, player.transform.parent.gameObject);

            spawner.SpawnCheese();

            // destroy this one
            NetworkServer.Destroy(gameObject);
        }
    }
}

