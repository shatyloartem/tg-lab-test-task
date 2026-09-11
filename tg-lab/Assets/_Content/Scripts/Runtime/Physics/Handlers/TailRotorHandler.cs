using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics.Handlers
{
    public sealed class TailRotorHandler : FlightHandler
    {
        public TailRotorHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings)
        { }

        public override void Step(ref FlightFrame frame)
        {
            float authority = FlightPhysicsMath.RotorAuthority(frame.Thrust, Settings._mass);
            float torqueLimit = Settings._maximumTailTorque * authority;
            float tailTorque = Mathf.Clamp(frame.Control.TailRotorTorque, -torqueLimit, torqueLimit);
            float compensation = CalculateMainRotorTorqueCompensation(frame);

            tailTorque = Mathf.Clamp(tailTorque + compensation, -torqueLimit, torqueLimit);

            Body.AddRelativeTorque(Vector3.up * tailTorque, ForceMode.Force);
        }

        private float CalculateMainRotorTorqueCompensation(FlightFrame frame)
        {
            bool compensationEnabled = 
                frame.IsRunning 
                && Settings.FlightAssists.IsEnabled(FlightAssistFeature.MainRotorTorqueCompensation);

            return compensationEnabled
                ? frame.Thrust * Settings._reactionTorquePerNewton
                : 0f;
        }
    }
}
