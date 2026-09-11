using Runtime.Input;

namespace Runtime.Physics
{
    public struct FlightFrame
    {
        public readonly FlightCommand Command;
        public readonly bool IsRunning;
        public readonly float DeltaTime;
        public FlightControl Control;
        public float Thrust;

        public FlightFrame(FlightCommand command, bool isRunning, float deltaTime)
        {
            Command = command;
            IsRunning = isRunning;
            DeltaTime = deltaTime;
            Control = default;
            Thrust = 0f;
        }
    }
}