using System.Collections.Generic;
using Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Dylan
{
    [RequireComponent(typeof(NavMeshAgent), typeof(ShipControl))]
    [DefaultExecutionOrder(5)]
    public class ShipAutopilot : MonoBehaviour
    {
        private const float StoppedSpeed = 0.05f;

        private enum Mode { Manual, Navigating, Braking }

        [SerializeField] private SteeringController steeringController;
        [SerializeField] private Stat shipSpeed;
        [SerializeField] private List<Transform> ports = new();
        [SerializeField, Min(0f)] private float stoppingDistance = 0.5f;

        private NavMeshAgent agent;
        private Mode mode;
        private Vector3 brakingVelocity;

        internal bool IsAutopilot => mode != Mode.Manual;

        void Awake() => agent = GetComponent<NavMeshAgent>();

        public void SelectPort(int index) => TryNavigateTo(ports[index]);

        public bool TryNavigateTo(Transform port)
        {
            if (steeringController.CurrentMode is
                SteeringController.Mode.Steering or SteeringController.Mode.HelmBraking)
                return false;

            if (!agent.isOnNavMesh || !agent.SetDestination(port.position)) return false;

            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            mode = Mode.Navigating;
            shipSpeed.SetValue(0f);
            return true;
        }

        public void CancelAutopilot()
        {
            if (mode != Mode.Navigating) return;

            brakingVelocity = agent.velocity;
            mode = Mode.Braking;
        }

        internal void TakeManualControl()
        {
            if (mode == Mode.Braking) ReleaseShip();
        }

        void Update()
        {
            if (!IsAutopilot) return;
            if (!agent.isOnNavMesh)
            {
                ReleaseShip();
                return;
            }
            if (mode == Mode.Navigating && agent.pathPending) return;

            if (!agent.hasPath) // Can't find a path
            {
                ReleaseShip();
                return;
            }

            if (mode == Mode.Braking)
            {
                brakingVelocity = Vector3.MoveTowards(
                    brakingVelocity, Vector3.zero, agent.acceleration * Time.deltaTime
                );
                if (brakingVelocity == Vector3.zero) ReleaseShip();
                else agent.velocity = brakingVelocity;
                return;
            }

            // Reached the destination
            if (agent.velocity.sqrMagnitude <= StoppedSpeed * StoppedSpeed &&
                agent.remainingDistance <= stoppingDistance + 0.01f)
                ReleaseShip();
        }

        void OnDisable()
        {
            if (IsAutopilot) ReleaseShip();
        }

        private void ReleaseShip()
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }
            mode = Mode.Manual;
        }
    }
}
