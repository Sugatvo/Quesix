using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotation : MonoBehaviour
{
    public Vector3 lockPos;
    private void Awake()
    {
        lockPos = transform.position;
    }
    void Update()
    {
        // Locks the rotation.
        transform.rotation = Quaternion.Euler(90, 0, 0);
        transform.position = lockPos;
    }
}
