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
    public float limit = 144f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public Vector3 beginPosition;

    public float rotation;
    private float startTime;

    float threshold = 10f;

    public Animator m_Animator;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        beginPosition = transform.position;
        m_Animator = transform.GetChild(1).GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (restartPosition)
        {
            rb.MovePosition(Vector3.Slerp(transform.position, beginPosition, Time.deltaTime));

            if (Vector3.Distance(transform.position, beginPosition) < 0.2f)
            {
                rb.MovePosition(beginPosition);
                if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    CmdSetRestartPositionFlag(false);
                }
                
            }
        }

        if (GirarDer)
        {
            Quaternion deltaRotation = Quaternion.Euler(0f, 90f * Time.deltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
            aux += 90f * Time.fixedDeltaTime;
            if (aux > 90f)
            {
                if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    CmdNotifyRotationToServer(rb.rotation);
                }
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
                if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    CmdNotifyRotationToServer(rb.rotation);
                }
                GirarIzq = false;
                aux = 0;
            }
        }

        if (Up)
        {
            float distCovered = (Time.time - startTime);
            float fractionOfJourney = distCovered / 1;

            if (2 > Vector3.Distance(startPosition, transform.position))
            {
                rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, fractionOfJourney));
            }
            else
            {
                Up = false;
                if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    CmdNotifyPositionToServer(transform.position);
                }
            }
        }

        if (Down)
        {
            float distCovered = (Time.time - startTime);
            float fractionOfJourney = distCovered / 1;

            if (2 > Vector3.Distance(startPosition, transform.position))
            {
                rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, fractionOfJourney));
            }
            else
            {
                Down = false;
                if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    CmdNotifyPositionToServer(transform.position);
                }
            }
        }
    }

    [Command]
    public void CmdSetRestartPositionFlag(bool flag)
    {
        restartPosition = flag;
        m_Animator.SetBool("isRestartingPosition", flag);
    }

    [Command]
    public void CmdNotifyPositionToServer(Vector3 currentPosition)
    {
        rb.position = currentPosition;
    }

    [Command]
    public void CmdNotifyRotationToServer(Quaternion currentRotation)
    {
        rb.rotation = currentRotation;
    }


    public void Avanzar()
    {
        startTime = Time.time;
        rotation = rb.rotation.eulerAngles.y;

        //Get the starting positions, the target position and the offset
        startPosition = rb.position;

        if (270f - threshold < rotation && rotation < 270f + threshold)
        {
            targetPosition = startPosition + new Vector3(0, 0, -2);
        }
        else if (-threshold < rotation && rotation < threshold)
        {
            targetPosition = startPosition + new Vector3(-2, 0, 0);
        }
        else if (90f - threshold < rotation && rotation < 90f + threshold)
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
        startTime = Time.time;
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

        targetPosition = LimitTargetPosition(targetPosition);
        Down = true;
    }

    public Vector3 LimitTargetPosition(Vector3 _targetPosition)
    {
        if (_targetPosition.x <= -9)
        {
            _targetPosition.x = -9;
        }
        if (_targetPosition.x >= 9)
        {
            _targetPosition.x = 9;
        }
        if (_targetPosition.z <= -9)
        {
            _targetPosition.z = -9;
        }
        if (_targetPosition.z >= 9)
        {
            _targetPosition.z = 9;
        }
        return _targetPosition;
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




