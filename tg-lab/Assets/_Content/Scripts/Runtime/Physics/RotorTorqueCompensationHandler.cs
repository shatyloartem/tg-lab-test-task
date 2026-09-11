using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class RotorTorqueCompensationHandler : FlightHandler
    {
        public RotorTorqueCompensationHandler(Rigidbody body, HelicopterSettings settings)
            : base(body, settings)
        {
        }

        public override void Step(ref FlightFrame frame)
        {
            if (!frame.IsRunning)
                return;

            if (!Settings.IsFlightAssistEnabled(FlightAssistFeature.MainRotorTorqueCompensation))
                return;

            frame.Control.MainRotorCompensationPerNewton =
                Settings._reactionTorquePerNewton;
        }
    }
}
