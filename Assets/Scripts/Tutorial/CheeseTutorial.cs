using UnityEngine;

public class CheeseTutorial : MonoBehaviour
{
    public bool available = true;
    int points;

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
            available = false;
            points = 1;

            if (ManagerPiloto.Instance)
            {
                ManagerPiloto.Instance.AddScore(points);
            }

            if (ManagerCopiloto.Instance)
            {
                ManagerCopiloto.Instance.AddScore(points);
            }

            Destroy(gameObject);
         
        }
    }
}


