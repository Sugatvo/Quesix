using UnityEngine;


namespace Mirror
{
    public class Player : NetworkBehaviour
    {
        public float speed = 5;
        public Rigidbody2D rb;

        // need to use FixedUpdate for rigidbody
        void FixedUpdate()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
            {
                rb.MovePosition(rb.position + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * speed * Time.fixedDeltaTime);
            }      
        }
    }
}


