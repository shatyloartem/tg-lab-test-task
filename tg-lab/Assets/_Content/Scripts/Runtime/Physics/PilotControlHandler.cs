using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class PilotControlHandler : FlightHandler
    {
        public PilotControlHandler(Rigidbody body, HelicopterSettings settings)
            : base(body, settings)
        {
        }

        public override void Step(ref FlightFrame frame)
        {
            frame.Control = default;

            if (!frame.IsRunning)
                return;

            frame.Control.MainRotorThrust = CalculateMainRotorThrust(frame.Command.Vertical);
            frame.Control.LocalCyclicTorque = CalculateCyclicTorque(frame.Command.Cyclic);
            frame.Control.TailRotorTorque = frame.Command.Yaw * Settings._maximumTailTorque;
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
            return direction * Settings._maximumCyclicTorque;
        }
    }
}
