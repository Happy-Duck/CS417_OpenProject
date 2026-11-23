using UnityEngine;

namespace _Sharyq
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hook : MonoBehaviour
    {
        [SerializeField] private bool inWater;
        [SerializeField] private float waterLevel = 0f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!inWater)
            {
                if (transform.position.y <= waterLevel)
                    EnterWater();
                return;
            }
            
            if (rb)
            {
                Vector3 pos = rb.position;
                pos.y = waterLevel;
                rb.position = pos;

                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                rb.linearVelocity = vel; 
            }
        }

        private void EnterWater()
        {
            inWater = true;
            if (!rb)
                return;
            rb.useGravity = false;

            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;
        }

        public void LeaveWater()
        {
            inWater = false;
            if (rb)
                rb.useGravity = true;
        }
    }
}