using Runtime.Configuration;
using Runtime.Input;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class MainRotorHandler : FlightHandler
    {
        private float targetAltitude;

        public float Thrust { get; private set; }

        public MainRotorHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings)
        {
            Reset();
        }

        public override void Step(ref FlightFrame frame)
        {
            Vector3 rotorUp = Body.rotation * Vector3.up;
            float desiredThrust = CalculateDesiredThrust(frame.Command, frame.IsRunning, rotorUp);
            Thrust = Mathf.Lerp(Thrust, desiredThrust,
                FlightPhysicsMath.Response(frame.DeltaTime, Settings.rotorResponseTime));

            frame.Thrust = Thrust;
            Body.AddForce(rotorUp * Thrust, ForceMode.Force);
            Body.AddTorque(rotorUp * (-Thrust * Settings.reactionTorquePerNewton), ForceMode.Force);
        }

        public override void Reset()
        {
            targetAltitude = Body.position.y;
            Thrust = 0f;
        }

        private float CalculateDesiredThrust(FlightCommand command, bool running, Vector3 rotorUp)
        {
            if (!running)
            {
                targetAltitude = Body.position.y;
                return 0f;
            }

            float targetSpeed;
            if (Mathf.Abs(command.Vertical) > 0.01f)
            {
                targetAltitude = Body.position.y;
                targetSpeed = command.Vertical * (command.Vertical > 0f
                    ? Settings.climbSpeed : Settings.descentSpeed);
            }
            else
            {
                targetSpeed = Mathf.Clamp((targetAltitude - Body.position.y) * Settings.altitudeGain,
                    -Settings.descentSpeed, Settings.climbSpeed);
            }

            float upright = Vector3.Dot(rotorUp, Vector3.up);
            if (upright <= 0.25f) return 0f;

            float acceleration = (targetSpeed - Body.linearVelocity.y) * Settings.verticalSpeedGain;
            return Mathf.Clamp(Settings.mass * (-UnityEngine.Physics.gravity.y + acceleration) / upright,
                0f, Settings.maximumThrust);
        }
    }
}
