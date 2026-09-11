using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics.Handlers
{
    public sealed class AerodynamicDragHandler : FlightHandler
    {
        public AerodynamicDragHandler(Rigidbody body, HelicopterSettings settings) : base(body, settings) { }

        public override void Step(ref FlightFrame frame)
        {
            Quaternion rotation = Body.rotation;
            Vector3 localVelocity = Quaternion.Inverse(rotation) * Body.linearVelocity;
            Vector3 localDrag = FlightPhysicsMath.Drag(localVelocity, Settings._dragArea, Settings._airDensity);
            Body.AddForce(rotation * localDrag, ForceMode.Force);
            Body.AddTorque(-Body.angularVelocity * Settings._rotationalDrag, ForceMode.Force);
        }
    }
}
