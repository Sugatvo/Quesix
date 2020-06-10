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
    public float moveSpeed = 150f;
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

    void Update()
    {

        if (!this.gameObject.GetComponent<NetworkIdentity>().hasAuthority)
            return;

        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == "Arriba")
        {
            Avanzar();
        }
        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == "Izquierda")
        {
            GirarIzquierda();
        }
        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == "Derecha")
        {
            GirarDerecha();
        }
        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == "Abajo")
        {
            Retroceder();
        }


        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Q and E cancel each other out, reducing the turn to zero
        if (Input.GetKey(KeyCode.Q))
            turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
        if (Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, 0, turnSensitivity);




    }

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
            direction = new Vector3(0, 0, -1);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;


            if (transform.position.z < -4.6 && direction.z < 0)
            {
                direction.z = 0;
            }

            if (transform.position.z > 4.6 && direction.z > 0)
            {
                direction.z = 0;
            }

            direction = direction * Time.fixedDeltaTime;
            characterController.SimpleMove(direction);

            Debug.Log(direction * Time.fixedDeltaTime);

            Debug.Log(aux);
            aux += direction.sqrMagnitude;
            Debug.Log(aux);
            if (aux > limit)
            {
                Up = false;
                aux = 0;
            }
            

        }

        if (Down)
        {
            direction = new Vector3(0, 0, 1);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (transform.position.z < -4.6 && direction.z < 0)
            {
                direction.z = 0;
            }

            if (transform.position.z > 4.6 && direction.z > 0)
            {
                direction.z = 0;
            }

            direction = direction * Time.fixedDeltaTime;
            characterController.SimpleMove(direction);


            Debug.Log(aux);
            aux += direction.sqrMagnitude;
            Debug.Log(aux);
            if (aux > limit)
            {
                Down = false;
                aux = 0;
            }
           
        }
         
        /*
        characterController.SimpleMove(direction);


        Vector3 direction = new Vector3();
        direction.x = horizontal;
        direction.y = 0;
        direction.z = vertical;

        direction = Vector3.ClampMagnitude(direction, 1f);
        direction = transform.TransformDirection(direction);
        direction *= -moveSpeed;


        if (transform.position.x < -4.6 && direction.x < 0)
        {
            direction.x = 0;
        }

        if (transform.position.x > 4.6 && direction.x > 0)
        {
            direction.x = 0;
        }

        if (transform.position.z < -4.6 && direction.z < 0)
        {
            direction.z = 0;
        }

        if (transform.position.z > 4.6 && direction.z > 0)
        {
            direction.z = 0;
        } */
    }

    public void Avanzar()
    {
        Up = true;
    }

    public void Retroceder()
    {
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




