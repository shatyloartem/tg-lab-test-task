using UnityEngine;

namespace Runtime.Physics
{
    public readonly struct FlightCommand
    {
        public readonly Vector2 Cyclic;
        public readonly float Vertical;
        public readonly float Yaw;

        public FlightCommand(Vector2 cyclic, float vertical, float yaw)
        {
            Cyclic = Vector2.ClampMagnitude(cyclic, 1f);
            Vertical = Mathf.Clamp(vertical, -1f, 1f);
            Yaw = Mathf.Clamp(yaw, -1f, 1f);
        }
    }
}
