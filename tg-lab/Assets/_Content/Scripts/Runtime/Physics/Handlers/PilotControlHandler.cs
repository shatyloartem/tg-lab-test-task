using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics.Handlers
{
    public sealed class PilotControlHandler : FlightHandler
    {
        private Vector2 _cyclicTarget;

        public PilotControlHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings)
        { }

        public override void Step(ref FlightFrame frame)
        {
            frame.Control = default;

            if (!frame.IsRunning)
            {
                Reset();
                return;
            }

            _cyclicTarget = Settings.FlightAssists.IsEnabled(FlightAssistFeature.CyclicInputFiltering)
                ? FilterCyclicInput(frame.Command.Cyclic, frame.DeltaTime)
                : frame.Command.Cyclic;

            frame.Control.CyclicTarget = _cyclicTarget;
            frame.Control.MainRotorThrust = CalculateMainRotorThrust(frame.Command.Vertical);
            frame.Control.LocalCyclicTorque = CalculateCyclicTorque(_cyclicTarget);
            frame.Control.TailRotorTorque = frame.Command.Yaw * Settings._maximumTailTorque;
        }

        public override void Reset() => _cyclicTarget = Vector2.zero;

        private Vector2 FilterCyclicInput(Vector2 pilotInput, float deltaTime)
        {
            var smoothingFactor = FlightPhysicsMath.ExpSmoothingFactor(deltaTime, Settings._cyclicResponseTime);

            return Vector2.Lerp(_cyclicTarget, pilotInput, smoothingFactor);
        }

        private float CalculateMainRotorThrust(float verticalInput)
        {
            float hoverThrust = Settings._mass * -UnityEngine.Physics.gravity.y;
            hoverThrust = Mathf.Min(hoverThrust, Settings._maximumThrust);

            return verticalInput >= 0f
                ? Mathf.Lerp(hoverThrust, Settings._maximumThrust, verticalInput)
                : Mathf.Lerp(hoverThrust, 0f, -verticalInput);
        }

        private Vector3 CalculateCyclicTorque(Vector2 cyclicInput)
        {
            Vector3 direction = new(cyclicInput.y, 0f, -cyclicInput.x);
            return direction * Settings._manualCyclicTorque;
        }
    }
}
