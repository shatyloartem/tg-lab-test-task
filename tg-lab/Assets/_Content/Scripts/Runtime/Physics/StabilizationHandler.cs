using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class StabilizationHandler : FlightHandler
    {
        private Vector2 cyclic;

        public StabilizationHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings) { }

        public override void Step(ref FlightFrame frame)
        {
            if (!frame.IsRunning)
            {
                cyclic = Vector2.zero;
                return;
            }

            cyclic = Vector2.Lerp(cyclic, frame.Command.Cyclic,
                FlightPhysicsMath.Response(frame.DeltaTime, Settings.cyclicResponseTime));

            Quaternion rotation = Body.rotation;
            Quaternion target = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f) *
                Quaternion.Euler(cyclic.y * Settings.maximumTilt, 0f,
                    -cyclic.x * Settings.maximumTilt);
            Quaternion error = target * Quaternion.Inverse(rotation);
            if (error.w < 0f)
                error = new Quaternion(-error.x, -error.y, -error.z, -error.w);

            error.ToAngleAxis(out float angle, out Vector3 axis);
            Vector3 rotationError = angle < 0.001f
                ? Vector3.zero
                : axis * (angle * Mathf.Deg2Rad);
            Vector3 yawVelocity = Vector3.Project(Body.angularVelocity, Vector3.up);
            Vector3 acceleration = rotationError * Settings.attitudeGain -
                (Body.angularVelocity - yawVelocity) * Settings.angularDamping;
            Vector3 torque = FlightPhysicsMath.InertiaTorque(acceleration,
                rotation * Body.inertiaTensorRotation, Body.inertiaTensor);
            Vector3 localTorque = Quaternion.Inverse(rotation) * torque;
            localTorque.y = 0f;

            float authority = Mathf.Clamp01(frame.Thrust / (Settings.mass * -UnityEngine.Physics.gravity.y));
            localTorque = Vector3.ClampMagnitude(localTorque, Settings.maximumCyclicTorque) * authority;
            Body.AddTorque(rotation * localTorque, ForceMode.Force);
        }

        public override void Reset() => cyclic = Vector2.zero;
    }
}
