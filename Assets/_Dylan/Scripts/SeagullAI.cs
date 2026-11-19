using UnityEngine;
using Shared;

namespace Dylan {
    public enum SeagullState
    {
        Circling, Chasing, Escaping
    }


    public class SeagullAI : MonoBehaviour
    {
        public Transform target;
        public Resource resource;
        public SeagullState currentState = SeagullState.Circling;

        [Header("Flight")]
        public float flySpeed = 5f;
        public float rotationSpeed = 180f;
        private Vector3 FlightForward => -transform.forward;  // The seagull model faces local -Z.
        private float CurrentSpeed => currentState == SeagullState.Escaping
            ? flySpeed * 0.5f : flySpeed;

        [Header("Circling")]
        public float circleRadius = 5f;
        public float circleSpeed = 60f; // Degrees per second
        private Vector3 circleCenter;
        private int circleDirection;

        [Header("Escaping")]
        public float escapeDistance = 20f;
        private Vector3 escapeDirection;
        private Vector3 escapeStartPosition;

        [Header("Stat")]
        private int hp = 3;
        public int Hp => hp;
        private bool hasResource = false;

        void Awake()
        {
            circleDirection = Random.value < 0.5f ? 1 : -1;
            StartCircling();
        }

        void Update()
        {
            CheckTarget();
            switch(currentState)
            {
                case SeagullState.Circling:
                    Circle();
                    break;
                case SeagullState.Chasing:
                    ChaseTarget();
                    break;
                case SeagullState.Escaping:
                    Escape();
                    break;
            }
        }

        void ChangeState(SeagullState newState)
        {
            if (currentState == newState) return;

            currentState = newState;

            switch (newState)
            {
                case SeagullState.Circling:
                    StartCircling();
                    break;
                case SeagullState.Chasing:
                    break;
                case SeagullState.Escaping:
                    StartEscaping();
                    break;
            }
        }

        void CheckTarget()
        {
            if (currentState == SeagullState.Escaping) return;
            if (target != null) ChangeState(SeagullState.Chasing);
            else ChangeState(SeagullState.Circling);
        }

        void ChaseTarget()
        {
            Vector3 direction = target.position - transform.position;
            if (direction.sqrMagnitude < 1e-4f) return;

            Quaternion targetRotation = Quaternion.LookRotation(-direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            transform.position += FlightForward * CurrentSpeed * Time.deltaTime;
        }

        void StartCircling() => circleCenter = transform.position - transform.right * circleRadius * circleDirection;

        void Circle() => transform.RotateAround(
            circleCenter,
            Vector3.up,
            circleDirection * circleSpeed * Time.deltaTime
            );

        void StartEscaping()
        {
            target = null;
            escapeStartPosition = transform.position;
            escapeDirection = -FlightForward;
            transform.rotation = Quaternion.LookRotation(-escapeDirection);
        }

        void Escape(){
            transform.rotation = Quaternion.LookRotation(-escapeDirection);
            transform.position += FlightForward * CurrentSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, escapeStartPosition) >= escapeDistance) 
                Destroy(gameObject);
        }

        void DropResourse()
        {
            if (resource != null && resource.Prefab != null) 
                Instantiate(resource.Prefab, transform.position, transform.rotation);
        }

        void OnTriggerEnter(Collider other) {
            switch(currentState)
            {
                case SeagullState.Circling:
                    ChangeState(SeagullState.Escaping);
                    break;
                case SeagullState.Chasing: 
                    if (other.CompareTag("Dish")) hasResource = true;
                    ChangeState(SeagullState.Escaping);
                    break;
                case SeagullState.Escaping:
                    hp -= 1;
                    if (hp > 0) break;
                    if (hasResource) DropResourse();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
