using System;
using Shared;
using UnityEngine;

namespace Dylan
{
    [Serializable]
    public sealed class ThrottleControl
    {
        private const float Step = 10f;
        private const float RepeatInterval = 0.5f;
        private const float DirectionThreshold = 0.5f;

        [SerializeField] private Stat shipSpeed;

        public float SpeedPercent => shipSpeed.Value;

        private int previousDirection;
        private float nextStepTime;

        internal void Begin() => previousDirection = 0;

        internal void UpdateThrottle(float axis)
        {
            int direction = Mathf.Abs(axis) >= DirectionThreshold ? (int)Mathf.Sign(axis) : 0;
            if (direction == 0)
            {
                previousDirection = 0;
                return;
            }

            float now = Time.time;
            // state change
            if (direction != previousDirection)
            {
                previousDirection = direction;
                nextStepTime = now;
            }

            if (now < nextStepTime) return;

            shipSpeed.Add(direction * Step);
            nextStepTime += RepeatInterval;
        }
    }
}
