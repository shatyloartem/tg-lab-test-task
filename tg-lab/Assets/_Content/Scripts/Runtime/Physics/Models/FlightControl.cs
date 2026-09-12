using UnityEngine;

namespace Runtime.Physics
{
    public struct FlightControl
    {
        public Vector2 CyclicTarget;
        public float MainRotorThrust;
        public Vector3 LocalCyclicTorque;
        public float TailRotorTorque;
    }
}
