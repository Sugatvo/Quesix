using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerController3D : NetworkBehaviour
{
    public CharacterController characterController;

    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    [Header("Movement Settings")]
    public float moveSpeed = 100f;
    public float turnSensitivity = 5f;
    public float maxTurnSpeed = 150f;

    [Header("Diagnostics")]
    public float horizontal;
    public float vertical;
    public float turn;
    public bool GirarDer = false;
    public bool GirarIzq = false;
    public bool Up = false;
    public bool Down = false;
    public float aux = 0f;
    public Vector3 velocity;
    Vector3 direction;
    public float limit = 144f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public float rotation;

    float threshold = 10f;

    void FixedUpdate()
    {
        if (!this.gameObject.GetComponent<NetworkIdentity>().hasAuthority || characterController == null)
            return;

        if (GirarDer)
        {
            transform.Rotate(0f, 90f * Time.fixedDeltaTime, 0f);
            aux += 90f * Time.fixedDeltaTime;
            if (aux > 90f)
            {
                GirarDer = false;
                aux = 0;
            }

        }

        if (GirarIzq)
        {
            transform.Rotate(0f, -90f * Time.fixedDeltaTime, 0f);
            aux += -90f * Time.fixedDeltaTime;
            if (aux < -90f)
            {
                GirarIzq = false;
                aux = 0;
            }
        }

        if (Up)
        {

            if (270f - threshold < rotation && rotation < 270f + threshold)
            {
                direction = new Vector3(0, 0, -1);
            }
            else if (-threshold < rotation && rotation < threshold)
            {
                direction = new Vector3(-1, 0, 0);
            }
            else if (90f - threshold < rotation && rotation < 90f + threshold)
            {
                direction = new Vector3(0, 0, 1);
            }
            else
            {
                direction = new Vector3(1, 0, 0);
            }
            /*
            if (transform.position.z < -9 && direction.z < 0)
            {
                direction.z = 0;
            }

            if (transform.position.z > 9 && direction.z > 0)
            {
                direction.z = 0;
            }*/

            if (2 > Vector3.Distance(startPosition, transform.position))
            {
                characterController.SimpleMove(direction * Time.deltaTime * moveSpeed);
            }
            else
            {
                Up = false;
            }
        }

        if (Down)
        {
            if (270f - threshold < rotation && rotation < 270f + threshold)
            {
                direction = new Vector3(0, 0, 1);
            }
            else if (-threshold < rotation && rotation < threshold)
            {
                direction = new Vector3(1, 0, 0);
            }
            else if (90f - threshold < rotation && rotation < 90f + threshold)
            {
                direction = new Vector3(0, 0, -1);
            }
            else
            {
                direction = new Vector3(-1, 0, 0);
            }

            /*
            if (transform.position.z < -4.6 && direction.z < 0)
            {
                direction.z = 0;
            }

            if (transform.position.z > 4.6 && direction.z > 0)
            {
                direction.z = 0;
            }*/

            if (2 > Vector3.Distance(startPosition, transform.position))
            {
                characterController.SimpleMove(direction * Time.deltaTime * moveSpeed);
            }
            else
            {
                Down = false;
            }
        }

    }

    public void Avanzar()
    {
        rotation = this.transform.rotation.eulerAngles.y;

        //Get the starting positions, the target position and the offset
        startPosition = transform.position;

        if(270f - threshold < rotation && rotation < 270f + threshold)
        {
            targetPosition = startPosition + new Vector3(0, 0, -2);
        }
        else if (-threshold < rotation && rotation < threshold)
        {
            targetPosition = startPosition + new Vector3(-2, 0, 0);
        }
        else if(90f - threshold < rotation && rotation < 90f + threshold)
        {
            targetPosition = startPosition + new Vector3(0, 0, 2);
        }
        else
        {
            targetPosition = startPosition + new Vector3(2, 0, 0);
        }


        Up = true;
    }

    public void Retroceder()
    {
        rotation = this.transform.rotation.eulerAngles.y;

        //Get the starting positions, the target position and the offset
        startPosition = transform.position;


        if (270f - threshold < rotation && rotation < 270f + threshold)
        {
            targetPosition = startPosition + new Vector3(0, 0, 2);
        }
        else if (-threshold < rotation && rotation < threshold)
        {
            targetPosition = startPosition + new Vector3(2, 0, 0);
        }
        else if (90f - threshold < rotation && rotation < 90f + threshold)
        {
            targetPosition = startPosition + new Vector3(0, 0, -2);
        }
        else
        {
            targetPosition = startPosition + new Vector3(-2, 0, 0);
        }

        Down = true;

    }

    public void GirarDerecha()
    {
        GirarDer = true;
        turn = 90f;
    }

    public void GirarIzquierda()
    {
        GirarIzq = true;
        turn = -90f;
    }

}




