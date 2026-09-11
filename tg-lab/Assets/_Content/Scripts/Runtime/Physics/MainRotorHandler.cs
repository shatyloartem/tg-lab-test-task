using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class MainRotorHandler : FlightHandler
    {
        public float Thrust { get; private set; }

        public MainRotorHandler(Rigidbody body, HelicopterSettings settings)
            : base(body, settings)
        {
        }

        public override void Step(ref FlightFrame frame)
        {
            float targetThrust = Mathf.Clamp(
                frame.Control.MainRotorThrust,
                0f,
                Settings._maximumThrust);
            float response = FlightPhysicsMath.Response(
                frame.DeltaTime,
                Settings._rotorResponseTime);

            Thrust = Mathf.Lerp(Thrust, targetThrust, response);
            frame.Thrust = Thrust;

            Vector3 rotorUp = Body.rotation * Vector3.up;
            float reactionTorque = -Thrust * Settings._reactionTorquePerNewton;
            Body.AddForce(rotorUp * Thrust, ForceMode.Force);
            Body.AddTorque(rotorUp * reactionTorque, ForceMode.Force);
        }

        public override void Reset() => Thrust = 0f;
    }
}
