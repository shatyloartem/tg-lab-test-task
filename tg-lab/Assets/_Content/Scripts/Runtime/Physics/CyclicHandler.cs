using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class CyclicHandler : FlightHandler
    {
        public CyclicHandler(Rigidbody body, HelicopterSettings settings)
            : base(body, settings)
        {
        }

        public override void Step(ref FlightFrame frame)
        {
            float authority = FlightPhysicsMath.RotorAuthority(frame.Thrust, Settings._mass);
            Vector3 localTorque = Vector3.ClampMagnitude(
                frame.Control.LocalCyclicTorque,
                Settings._maximumCyclicTorque);

            Body.AddRelativeTorque(localTorque * authority, ForceMode.Force);
        }
    }
}
