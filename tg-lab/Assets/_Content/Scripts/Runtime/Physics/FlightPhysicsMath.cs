using UnityEngine;

namespace Runtime.Physics
{
    public static class FlightPhysicsMath
    {
        public static float Response(float deltaTime, float responseTime) =>
            1f - Mathf.Exp(-deltaTime / Mathf.Max(0.01f, responseTime));

        public static Vector3 Drag(Vector3 airVelocity, Vector3 dragArea, float density)
        {
            return -0.5f * density * Vector3.Scale(dragArea, new Vector3(
                airVelocity.x * Mathf.Abs(airVelocity.x),
                airVelocity.y * Mathf.Abs(airVelocity.y),
                airVelocity.z * Mathf.Abs(airVelocity.z)));
        }

        public static Vector3 InertiaTorque(Vector3 angularAcceleration,
            Quaternion principalRotation, Vector3 inertiaTensor)
        {
            return principalRotation * Vector3.Scale(inertiaTensor,
                Quaternion.Inverse(principalRotation) * angularAcceleration);
        }
    }
}
