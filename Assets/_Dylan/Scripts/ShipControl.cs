using UnityEngine;

namespace Dylan
{
    [DefaultExecutionOrder(10)]
    public class ShipControl : MonoBehaviour
    {        
        [SerializeField] private SteeringController helm;
        [SerializeField, Min(0f)] private float turnRate = 30f; // turning speed
        void LateUpdate()
        {
            if (helm.IsSteering)
                transform.Rotate(
                    Vector3.up, helm.SteeringInput * turnRate * Time.deltaTime, Space.World
                );
        }
    }
}
