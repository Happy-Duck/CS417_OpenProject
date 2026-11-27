using UnityEngine;

namespace Alyssa
{
    public class FinishedRoll : MonoBehaviour
    {
        public RollData roll;

        // Called by NoriRoller right after the roll spawns
        public void Init(RollData data)
        {
            roll = data;

            SliceStepper stepper = GetComponent<SliceStepper>();
            if (stepper != null && data != null)
                stepper.Configure(data.slicePrefab, data.piecesWhenSliced);
        }

        void Start()
        {
            // Lets a roll placed in the scene for testing work too (does nothing if already set up)
            if (roll != null) Init(roll);
        }
    }
}