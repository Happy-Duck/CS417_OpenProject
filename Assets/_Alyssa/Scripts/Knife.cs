using UnityEngine;

namespace Alyssa
{
    public class Knife : MonoBehaviour
    {
        public Transform bladePoint;
        public KnifeType toolType;
        [SerializeField] float debugSpeed;

        public float Speed { get; private set; }

        Vector3 lastPos;
        float holdUntil;

        void Start()
        {
            if (bladePoint == null) bladePoint = transform;
            lastPos = bladePoint.position;
        }

        void Update()
        {
            if (Time.deltaTime <= 0f) return;

            float instant = (bladePoint.position - lastPos).magnitude / Time.deltaTime;
            lastPos = bladePoint.position;

            // Hold the peak briefly so a fast swing still counts on the next physics step
            if (instant >= Speed || Time.time > holdUntil)
            {
                Speed = instant;
                holdUntil = Time.time + 0.1f;
            }

            debugSpeed = Speed;
        }

        void OnTriggerEnter(Collider other)
        {
            CutZone zone = other.GetComponent<CutZone>();
            if (zone != null)
            {
                zone.Hit(Speed, toolType);
                return;
            }

            Cuttable target = other.GetComponentInParent<Cuttable>();
            if (target != null) target.TryHit(Speed, toolType, false);
        }
    }
}