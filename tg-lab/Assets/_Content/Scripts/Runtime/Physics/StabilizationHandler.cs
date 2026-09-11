using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class StabilizationHandler : FlightHandler
    {
        private const float InputDeadZone = 0.01f;
        private const float MinimumRotationErrorDegrees = 0.001f;

        private Vector2 cyclicTarget;
        private float targetAltitude;
        private float targetHeading;

        public StabilizationHandler(Rigidbody body, HelicopterSettings settings)
            : base(body, settings)
        {
            Reset();
        }

        public override void Step(ref FlightFrame frame)
        {
            if (!frame.IsRunning)
            {
                Reset();
                return;
            }

            FlightAssistSettings assists = Settings.FlightAssists;

            cyclicTarget = assists.IsEnabled(FlightAssistFeature.CyclicInputSmoothing)
                ? SmoothCyclicTarget(frame.Command.Cyclic, frame.DeltaTime)
                : frame.Command.Cyclic;

            if (assists.IsEnabled(FlightAssistFeature.AttitudeStabilization))
                frame.Control.LocalCyclicTorque = CalculateAttitudeTorque();

            if (assists.IsEnabled(FlightAssistFeature.AngularVelocityDamping))
                frame.Control.LocalCyclicTorque += CalculateAngularDampingTorque();

            if (assists.IsEnabled(FlightAssistFeature.AltitudeHold))
                frame.Control.MainRotorThrust = CalculateAltitudeHoldThrust(frame.Command.Vertical);
            else
                targetAltitude = Body.position.y;

            if (assists.IsEnabled(FlightAssistFeature.HeadingHold))
                frame.Control.TailRotorTorque = CalculateHeadingHoldTorque(frame.Command.Yaw, frame.DeltaTime);
            else
                targetHeading = Body.rotation.eulerAngles.y;
        }

        public override void Reset()
        {
            cyclicTarget = Vector2.zero;
            targetAltitude = Body.position.y;
            targetHeading = Body.rotation.eulerAngles.y;
        }

        private Vector2 SmoothCyclicTarget(Vector2 pilotInput, float deltaTime)
        {
            float response = FlightPhysicsMath.Response(deltaTime, Settings._cyclicResponseTime);

            return Vector2.Lerp(cyclicTarget, pilotInput, response);
        }

        private Vector3 CalculateAttitudeTorque()
        {
            Quaternion rotation = Body.rotation;
            Vector3 angularAcceleration = CalculateAttitudeAcceleration(rotation);

            return CalculateLocalCyclicTorque(angularAcceleration, rotation);
        }

        private Vector3 CalculateAttitudeAcceleration(Quaternion rotation)
        {
            Quaternion heading = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            Quaternion tilt = Quaternion.Euler(
                cyclicTarget.y * Settings._maximumTilt,
                0f,
                -cyclicTarget.x * Settings._maximumTilt);
            Quaternion error = heading * tilt * Quaternion.Inverse(rotation);

            if (error.w < 0f)
                error = new Quaternion(-error.x, -error.y, -error.z, -error.w);

            error.ToAngleAxis(out float angle, out Vector3 axis);
            Vector3 rotationError = angle < MinimumRotationErrorDegrees
                ? Vector3.zero
                : axis * (angle * Mathf.Deg2Rad);

            return rotationError * Settings._attitudeGain;
        }

        private Vector3 CalculateAngularDampingTorque()
        {
            Vector3 yawVelocity = Vector3.Project(Body.angularVelocity, Vector3.up);
            Vector3 cyclicVelocity = Body.angularVelocity - yawVelocity;
            Vector3 angularAcceleration = -cyclicVelocity * Settings._angularDamping;

            return CalculateLocalCyclicTorque(angularAcceleration, Body.rotation);
        }

        private Vector3 CalculateLocalCyclicTorque(
            Vector3 angularAcceleration,
            Quaternion rotation)
        {
            Quaternion inertiaRotation = rotation * Body.inertiaTensorRotation;
            Vector3 worldTorque = FlightPhysicsMath.InertiaTorque(
                angularAcceleration,
                inertiaRotation,
                Body.inertiaTensor);
            Vector3 localTorque = Quaternion.Inverse(rotation) * worldTorque;
            localTorque.y = 0f;

            return localTorque;
        }

        private float CalculateAltitudeHoldThrust(float verticalInput)
        {
            float targetVerticalSpeed;
            if (Mathf.Abs(verticalInput) > InputDeadZone)
            {
                targetAltitude = Body.position.y;
                float speedLimit = verticalInput > 0f
                    ? Settings._climbSpeed
                    : Settings._descentSpeed;
                targetVerticalSpeed = verticalInput * speedLimit;
            }
            else
            {
                targetVerticalSpeed = Mathf.Clamp(
                    (targetAltitude - Body.position.y) * Settings._altitudeGain,
                    -Settings._descentSpeed,
                    Settings._climbSpeed);
            }

            Vector3 rotorUp = Body.rotation * Vector3.up;
            float uprightFactor = Vector3.Dot(rotorUp, Vector3.up);
            if (uprightFactor <= 0.25f)
                return 0f;

            float verticalAcceleration =
                (targetVerticalSpeed - Body.linearVelocity.y) * Settings._verticalSpeedGain;
            float requiredAcceleration = -UnityEngine.Physics.gravity.y + verticalAcceleration;
            float desiredThrust = Settings._mass * requiredAcceleration / uprightFactor;

            return Mathf.Clamp(desiredThrust, 0f, Settings._maximumThrust);
        }

        private float CalculateHeadingHoldTorque(float yawInput, float deltaTime)
        {
            targetHeading = Mathf.Repeat(
                targetHeading + yawInput * Settings._yawSpeed * deltaTime,
                360f);

            float currentHeading = Body.rotation.eulerAngles.y;
            float headingError =
                Mathf.DeltaAngle(currentHeading, targetHeading) * Mathf.Deg2Rad;
            float targetRate = yawInput * Settings._yawSpeed * Mathf.Deg2Rad;
            float currentRate = Vector3.Dot(Body.angularVelocity, Vector3.up);
            float headingCorrection = headingError * Settings._attitudeGain;
            float rateCorrection = (targetRate - currentRate) * Settings._angularDamping;
            float angularAcceleration = headingCorrection + rateCorrection;
            Vector3 worldTorque = FlightPhysicsMath.InertiaTorque(
                Vector3.up * angularAcceleration,
                Body.rotation * Body.inertiaTensorRotation,
                Body.inertiaTensor);

            return Vector3.Dot(worldTorque, Body.rotation * Vector3.up);
        }
    }
}
