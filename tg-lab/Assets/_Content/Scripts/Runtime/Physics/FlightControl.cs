using UnityEngine;

namespace Runtime.Physics
{
    public struct FlightControl
    {
        public float MainRotorThrust;
        public Vector3 LocalCyclicTorque;
        public float TailRotorTorque;
        public float MainRotorCompensationPerNewton;
    }
}
