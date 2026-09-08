using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class YawHandler : FlightHandler
    {
        private float targetHeading;

        public YawHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings)
        {
            Reset();
        }

        public override void Step(ref FlightFrame frame)
        {
            if (!frame.IsRunning)
            {
                targetHeading = Body.rotation.eulerAngles.y;
                return;
            }

            targetHeading = Mathf.Repeat(targetHeading +
                frame.Command.Yaw * Settings.yawSpeed * frame.DeltaTime, 360f);
            float headingError = Mathf.DeltaAngle(Body.rotation.eulerAngles.y, targetHeading) * Mathf.Deg2Rad;
            float targetRate = frame.Command.Yaw * Settings.yawSpeed * Mathf.Deg2Rad;
            float currentRate = Vector3.Dot(Body.angularVelocity, Vector3.up);
            float acceleration = headingError * Settings.attitudeGain +
                (targetRate - currentRate) * Settings.angularDamping;
            Vector3 torque = FlightPhysicsMath.InertiaTorque(Vector3.up * acceleration,
                Body.rotation * Body.inertiaTensorRotation, Body.inertiaTensor);

            float authority = Mathf.Clamp01(frame.Thrust / (Settings.mass * -UnityEngine.Physics.gravity.y));
            float tailTorque = Mathf.Clamp(Vector3.Dot(torque, Body.rotation * Vector3.up),
                -Settings.maximumTailTorque * authority,
                Settings.maximumTailTorque * authority);

            // Tail torque also compensates the main rotor's reaction torque.
            float compensation = frame.Thrust * Settings.reactionTorquePerNewton;
            tailTorque = Mathf.Clamp(tailTorque + compensation,
                -Settings.maximumTailTorque * authority,
                Settings.maximumTailTorque * authority);
            Body.AddTorque(Body.rotation * Vector3.up * tailTorque, ForceMode.Force);
        }

        public override void Reset() => targetHeading = Body.rotation.eulerAngles.y;
    }
}
