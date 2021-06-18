using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraptTutorial : MonoBehaviour
{
    public bool available = true;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            FallIntoTrap(col.gameObject);
        }
    }

    public void FallIntoTrap(GameObject playerBody)
    {
        if (available)
        {
            // This is a fast switch to prevent two players claiming the prize in a bang-bang close contest for it.
            // First hit turns it off, pending the object being destroyed a few frames later.
            available = false;

            playerBody.transform.parent.GetComponent<PlayerController3DTutorial>().restartPosition = true;
            playerBody.transform.parent.GetComponent<PlayerController3DTutorial>().m_Animator.SetBool("isRestartingPosition", true);

            Destroy(gameObject);
            
        }
    }
}
