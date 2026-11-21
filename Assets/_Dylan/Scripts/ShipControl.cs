using UnityEngine;

namespace Dylan
{
    [DefaultExecutionOrder(10)]
    public class ShipControl : MonoBehaviour
    {
        [SerializeField] private SteeringController helm;
        [SerializeField, Min(0f)] private float turnRate = 30f;
        [SerializeField, Min(0f)] private float fullSpeed = 2f;

        void LateUpdate()
        {
            if (helm.IsSteering)
                transform.Rotate(
                    Vector3.up, helm.SteeringInput * turnRate * Time.deltaTime, Space.World
                );

            float speed = fullSpeed * helm.SpeedPercent / 100f;
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
