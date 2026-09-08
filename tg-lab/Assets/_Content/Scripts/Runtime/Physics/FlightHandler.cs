using Runtime.Configuration;
using Runtime.Input;
using UnityEngine;

namespace Runtime.Physics
{
    public abstract class FlightHandler
    {
        protected readonly Rigidbody Body;
        protected readonly HelicopterSettings Settings;
        private bool enabled = true;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                Reset();
            }
        }

        protected FlightHandler(Rigidbody body, HelicopterSettings settings)
        {
            Body = body;
            Settings = settings;
        }

        public abstract void Step(ref FlightFrame frame);
        public virtual void Reset() { }
    }

    public struct FlightFrame
    {
        public readonly FlightCommand Command;
        public readonly bool IsRunning;
        public readonly float DeltaTime;
        public float Thrust;

        public FlightFrame(FlightCommand command, bool isRunning, float deltaTime)
        {
            Command = command;
            IsRunning = isRunning;
            DeltaTime = deltaTime;
            Thrust = 0f;
        }
    }
}
