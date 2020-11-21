using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

[RequireComponent(typeof(NetworkTransform))]
public class PlayerController3D : NetworkBehaviour
{
    private Rigidbody rb;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Diagnostics")]
    public float horizontal;
    public float vertical;
    public float turn;
    public bool GirarDer = false;
    public bool GirarIzq = false;
    public bool Up = false;
    public bool Down = false;

    [SyncVar]
    public bool restartPosition = false;
    public float aux = 0f;
    public Vector3 velocity;
    Vector3 direction;
    public float limit = 144f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public Vector3 beginPosition;

    public float rotation;

    float threshold = 10f;

    public Animator m_Animator;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        beginPosition = transform.position;
        m_Animator = transform.GetChild(1).GetComponent<Animator>();
    }

    private void Update()
    {
        if(rb != null)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Avanzar();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Retroceder();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                GirarIzquierda();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                GirarDerecha();
            }
        }
    }

    void FixedUpdate()
    {

        if (!this.gameObject.GetComponent<NetworkIdentity>().hasAuthority || rb == null)
            return;


        if (restartPosition)
        {
            rb.MovePosition(Vector3.Slerp(transform.position, beginPosition, 1f * Time.deltaTime));

            if (Vector3.Distance(transform.position, beginPosition) < 0.1f)
            {
                rb.MovePosition(beginPosition);
                restartPosition = false;
                m_Animator.SetBool("isRestartingPosition", false);
            }
        }

        if (GirarDer)
        {
            Quaternion deltaRotation = Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
            aux += 90f * Time.fixedDeltaTime;
            if (aux > 90f)
            {
                GirarDer = false;
                aux = 0;
            }
        }

        if (GirarIzq)
        {
            Quaternion deltaRotation = Quaternion.Euler(0f, -90f * Time.deltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
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
                if(transform.position.z > -9)
                {
                    direction = new Vector3(0, 0, -1);
                }
                else
                {
                    Up = false;
                }
                
            }
            else if (-threshold < rotation && rotation < threshold)
            {
                if(transform.position.x > -9)
                {
                    direction = new Vector3(-1, 0, 0);
                }
                else
                {
                    Up = false;
                }
                
            }
            else if (90f - threshold < rotation && rotation < 90f + threshold)
            {
                if(transform.position.z < 9)
                {
                    direction = new Vector3(0, 0, 1);
                }
                else
                {
                    Up = false;
                }
            }
            else
            {
                if(transform.position.x < 9)
                {
                    direction = new Vector3(1, 0, 0);
                }
                else
                {
                    Up = false;
                }
                
            }

            Vector3 movePos = transform.position + direction * Time.deltaTime * moveSpeed;

            if (2 > Vector3.Distance(startPosition, transform.position))
            {
                rb.MovePosition(movePos);   
            }
            else
            {
                rb.MovePosition(targetPosition);
                Up = false;
            }
        }

        if (Down)
        {
            if (270f - threshold < rotation && rotation < 270f + threshold)
            {
                if(transform.position.z < 9)
                {
                    direction = new Vector3(0, 0, 1);
                }
                else
                {
                    Down = false;
                }

            }
            else if (-threshold < rotation && rotation < threshold)
            {
                if(transform.position.x < 9)
                {
                    direction = new Vector3(1, 0, 0);
                }
                else
                {
                    Down = false;
                }
                
            }
            else if (90f - threshold < rotation && rotation < 90f + threshold)
            {
                if(transform.position.z > -9)
                {
                    direction = new Vector3(0, 0, -1);
                }
                else
                {
                    Down = false;
                }
                
            }
            else
            {
                if(transform.position.x > -9)
                {
                   direction = new Vector3(-1, 0, 0);
                }
                else
                {
                    Down = false;
                }
                
            }


            Vector3 movePos = transform.position + direction * Time.deltaTime * moveSpeed;

            if (2 > Vector3.Distance(startPosition, rb.position))
            {
                rb.MovePosition(movePos);
            }
            else
            {
                rb.MovePosition(targetPosition);
                Down = false;
            }
        }

    }

    public void Avanzar()
    {
        rotation = rb.rotation.eulerAngles.y;

        //Get the starting positions, the target position and the offset
        startPosition = rb.position;

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
        rotation = rb.rotation.eulerAngles.y;

        //Get the starting positions, the target position and the offset
        startPosition = rb.position;


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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.gameObject);

            foreach (var item in transform.GetComponent<PlayerScoreQuesix>().equipo)
            {
                TargetHidePlayer(item.connectionToClient, this.gameObject, other.transform.parent.gameObject);
            }

        }

    }

    [TargetRpc]
    public void TargetHidePlayer(NetworkConnection target, GameObject playerTrigger, GameObject otherPlayer)
    {
        if(GameObject.ReferenceEquals(target.identity.gameObject.GetComponent<CameraController>().teamObject, playerTrigger))
        {
            // Hide other players
            otherPlayer.transform.GetChild(1).gameObject.SetActive(false);
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.gameObject);

            foreach (var item in transform.GetComponent<PlayerScoreQuesix>().equipo)
            {
                TargetShowPlayer(item.connectionToClient, this.gameObject, other.transform.parent.gameObject);
            }

        }
    }

    [TargetRpc]
    public void TargetShowPlayer(NetworkConnection target, GameObject playerTrigger, GameObject otherPlayer)
    {
        if (GameObject.ReferenceEquals(target.identity.gameObject.GetComponent<CameraController>().teamObject, playerTrigger))
        {
            // Show other players
            otherPlayer.transform.GetChild(1).gameObject.SetActive(true);
        }

    }


}




