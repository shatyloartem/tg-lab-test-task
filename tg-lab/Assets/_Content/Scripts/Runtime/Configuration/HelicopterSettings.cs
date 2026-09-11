using UnityEngine;

namespace Runtime.Configuration
{
    [CreateAssetMenu(menuName = "Helicopter/Flight Settings")]
    public sealed class HelicopterSettings : ScriptableObject
    {
        [Header("Body")]
        [Min(1f)] public float _mass = 800f;
        public Vector3 _centerOfMass = new(0f, -0.25f, 0f);

        [Header("Rotor forces")]
        [Min(1f)] public float _maximumThrust = 17000f;
        [Min(0.01f)] public float _rotorResponseTime = 0.3f;
        [Min(0f)] public float _reactionTorquePerNewton = 0.08f;
        [Min(1f)] public float _maximumCyclicTorque = 14000f;
        [Min(1f)] public float _maximumTailTorque = 9000f;

        [Header("Pilot limits")]
        [Range(1f, 60f)] public float _maximumTilt = 23f;
        [Min(1f)] public float _yawSpeed = 65f;
        [Min(0.1f)] public float _climbSpeed = 4f;
        [Min(0.1f)] public float _descentSpeed = 2.5f;
        [Min(0.01f)] public float _cyclicResponseTime = 0.2f;

        [Header("Flight assists")]
        [SerializeField] private FlightAssistSettings _flightAssists = new();

        [Header("Stabilization")]
        [Min(0f)] public float _attitudeGain = 12f;
        [Min(0f)] public float _angularDamping = 6f;
        [Min(0f)] public float _altitudeGain = 1.2f;
        [Min(0f)] public float _verticalSpeedGain = 3f;

        [Header("Aerodynamics")]
        [Min(0f)] public float _airDensity = 1.225f;
        [Tooltip("Drag coefficient multiplied by frontal area, in body X/Y/Z axes (m²).")]
        public Vector3 _dragArea = new(5f, 8f, 2.5f);
        [Min(0f)] public float _rotationalDrag = 220f;

        public FlightAssistSettings FlightAssists => _flightAssists ??= new FlightAssistSettings();

        public bool IsFlightAssistEnabled(FlightAssistFeature feature) => FlightAssists.IsEnabled(feature);
    }
}
