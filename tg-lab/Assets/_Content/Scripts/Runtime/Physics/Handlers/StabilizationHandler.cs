using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics.Handlers
{
    public sealed class StabilizationHandler : FlightHandler
    {
        private const float InputDeadZone = 0.01f;
        private const float MinimumRotationErrorDegrees = 0.001f;
        private const float MinimumUprightFactor = 0.25f;

        private float _targetAltitude;
        private float _targetHeading;

        public StabilizationHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings)
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

            ApplyCyclicStabilization(ref frame.Control, assists);

            ApplyAltitudeHold(
                ref frame.Control,
                frame.Command.Vertical,
                assists.IsEnabled(FlightAssistFeature.AltitudeHold));

            ApplyHeadingHold(
                ref frame.Control,
                frame.Command.Yaw,
                frame.DeltaTime,
                assists.IsEnabled(FlightAssistFeature.HeadingHold));
        }

        public override void Reset()
        {
            _targetAltitude = Body.position.y;
            _targetHeading = Body.rotation.eulerAngles.y;
        }

        private void ApplyCyclicStabilization(ref FlightControl control, FlightAssistSettings assists)
        {
            if (assists.IsEnabled(FlightAssistFeature.AttitudeStabilization))
                control.LocalCyclicTorque = CalculateAttitudeTorque(control.CyclicTarget);

            if (assists.IsEnabled(FlightAssistFeature.AngularVelocityDamping))
                control.LocalCyclicTorque += CalculateAngularDampingTorque();
        }

        private void ApplyAltitudeHold(ref FlightControl control, float verticalInput, bool enabled)
        {
            if (enabled)
            {
                control.MainRotorThrust = CalculateAltitudeHoldThrust(verticalInput);
                return;
            }

            _targetAltitude = Body.position.y;
        }

        private void ApplyHeadingHold(
            ref FlightControl control,
            float yawInput,
            float deltaTime,
            bool enabled)
        {
            if (enabled)
            {
                control.TailRotorTorque = CalculateHeadingHoldTorque(yawInput, deltaTime);
                return;
            }

            _targetHeading = Body.rotation.eulerAngles.y;
        }

        #region Attitude stabilization

        private Vector3 CalculateAttitudeTorque(Vector2 cyclicInput)
        {
            Quaternion rotation = Body.rotation;
            Vector3 angularAcceleration = CalculateAttitudeAcceleration(cyclicInput, rotation);

            return CalculateLocalCyclicTorque(angularAcceleration, rotation);
        }

        private Vector3 CalculateAttitudeAcceleration(Vector2 cyclicInput, Quaternion currentRotation)
        {
            Quaternion targetRotation = CalculateTargetAttitude(cyclicInput, currentRotation);
            Quaternion rotationError = targetRotation * Quaternion.Inverse(currentRotation);
            rotationError = UseShortestRotation(rotationError);

            return ToRotationVector(rotationError) * Settings._attitudeGain;
        }

        private Quaternion CalculateTargetAttitude(Vector2 cyclicInput, Quaternion currentRotation)
        {
            Quaternion heading = Quaternion.Euler(0f, currentRotation.eulerAngles.y, 0f);
            Quaternion tilt = Quaternion.Euler(
                cyclicInput.y * Settings._maximumTilt,
                0f,
                -cyclicInput.x * Settings._maximumTilt);

            return heading * tilt;
        }

        private static Quaternion UseShortestRotation(Quaternion rotation)
        {
            return rotation.w >= 0f
                ? rotation
                : new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
        }

        private static Vector3 ToRotationVector(Quaternion rotation)
        {
            rotation.ToAngleAxis(out float angle, out Vector3 axis);

            return angle < MinimumRotationErrorDegrees
                ? Vector3.zero
                : axis * (angle * Mathf.Deg2Rad);
        }

        #endregion

        #region Angular damping and torque conversion

        private Vector3 CalculateAngularDampingTorque()
        {
            Vector3 yawVelocity = Vector3.Project(Body.angularVelocity, Vector3.up);
            Vector3 cyclicVelocity = Body.angularVelocity - yawVelocity;
            Vector3 angularAcceleration = -cyclicVelocity * Settings._angularDamping;

            return CalculateLocalCyclicTorque(angularAcceleration, Body.rotation);
        }

        private Vector3 CalculateLocalCyclicTorque(Vector3 angularAcceleration, Quaternion rotation)
        {
            Vector3 worldTorque = CalculateWorldTorque(angularAcceleration, rotation);
            Vector3 localTorque = Quaternion.Inverse(rotation) * worldTorque;
            localTorque.y = 0f;

            return localTorque;
        }

        private Vector3 CalculateWorldTorque(Vector3 angularAcceleration, Quaternion rotation)
        {
            return FlightPhysicsMath.InertiaTorque(
                angularAcceleration,
                rotation * Body.inertiaTensorRotation,
                Body.inertiaTensor);
        }

        #endregion

        #region Altitude hold

        private float CalculateAltitudeHoldThrust(float verticalInput)
        {
            float targetVerticalSpeed = CalculateTargetVerticalSpeed(verticalInput);
            Vector3 rotorUp = Body.rotation * Vector3.up;
            float uprightFactor = Vector3.Dot(rotorUp, Vector3.up);
            if (uprightFactor <= MinimumUprightFactor)
                return 0f;

            float verticalAcceleration = (targetVerticalSpeed - Body.linearVelocity.y) * Settings._verticalSpeedGain;
            float requiredAcceleration = -UnityEngine.Physics.gravity.y + verticalAcceleration;
            float desiredThrust = Settings._mass * requiredAcceleration / uprightFactor;

            return Mathf.Clamp(desiredThrust, 0f, Settings._maximumThrust);
        }

        private float CalculateTargetVerticalSpeed(float verticalInput)
        {
            if (Mathf.Abs(verticalInput) <= InputDeadZone)
            {
                return Mathf.Clamp(
                    (_targetAltitude - Body.position.y) * Settings._altitudeGain,
                    -Settings._descentSpeed,
                    Settings._climbSpeed);
            }

            _targetAltitude = Body.position.y;

            float speedLimit = verticalInput > 0f
                ? Settings._climbSpeed
                : Settings._descentSpeed;

            return verticalInput * speedLimit;
        }

        #endregion

        #region Heading hold

        private float CalculateHeadingHoldTorque(float yawInput, float deltaTime)
        {
            _targetHeading = Mathf.Repeat(
                _targetHeading + yawInput * Settings._yawSpeed * deltaTime,
                360f);

            float currentHeading = Body.rotation.eulerAngles.y;
            float headingError =
                Mathf.DeltaAngle(currentHeading, _targetHeading) * Mathf.Deg2Rad;
            float targetRate = yawInput * Settings._yawSpeed * Mathf.Deg2Rad;
            float currentRate = Vector3.Dot(Body.angularVelocity, Vector3.up);
            float headingCorrection = headingError * Settings._attitudeGain;
            float rateCorrection = (targetRate - currentRate) * Settings._angularDamping;
            float angularAcceleration = headingCorrection + rateCorrection;
            Vector3 worldTorque = CalculateWorldTorque(
                Vector3.up * angularAcceleration,
                Body.rotation);

            return Vector3.Dot(worldTorque, Body.rotation * Vector3.up);
        }

        #endregion
    }
}
