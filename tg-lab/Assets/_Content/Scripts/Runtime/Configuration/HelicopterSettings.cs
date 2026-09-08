using UnityEngine;

namespace Runtime.Configuration
{
    [CreateAssetMenu(menuName = "Helicopter/Flight Settings")]
    public sealed class HelicopterSettings : ScriptableObject
    {
        [Header("Body")]
        [Min(1f)] public float mass = 800f;
        public Vector3 centerOfMass = new(0f, -0.25f, 0f);

        [Header("Rotor forces (N and Nm)")]
        [Min(1f)] public float maximumThrust = 17000f;
        [Min(0.01f)] public float rotorResponseTime = 0.3f;
        [Min(0f)] public float reactionTorquePerNewton = 0.08f;
        [Min(1f)] public float maximumCyclicTorque = 14000f;
        [Min(1f)] public float maximumTailTorque = 9000f;

        [Header("Pilot limits")]
        [Range(1f, 60f)] public float maximumTilt = 23f;
        [Min(1f)] public float yawSpeed = 65f;
        [Min(0.1f)] public float climbSpeed = 4f;
        [Min(0.1f)] public float descentSpeed = 2.5f;
        [Min(0.01f)] public float cyclicResponseTime = 0.2f;

        [Header("Stabilization")]
        [Min(0f)] public float attitudeGain = 12f;
        [Min(0f)] public float angularDamping = 6f;
        [Min(0f)] public float altitudeGain = 1.2f;
        [Min(0f)] public float verticalSpeedGain = 3f;

        [Header("Aerodynamics")]
        [Min(0f)] public float airDensity = 1.225f;
        [Tooltip("Drag coefficient multiplied by frontal area, in body X/Y/Z axes (m²).")]
        public Vector3 dragArea = new Vector3(5f, 8f, 2.5f);
        [Min(0f)] public float rotationalDrag = 220f;
    }
}
