using Runtime.Configuration;
using Runtime.Input;
using Runtime.Physics;
using UnityEngine;

namespace Runtime.Controllers
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class HelicopterController : MonoBehaviour
    {
        [SerializeField] private HelicopterSettings _settings;

        private Rigidbody body;
        private FlightHandler[] handlers;
        private MainRotorHandler mainRotor;
        private GroundContactHandler groundContact = new();
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public FlightCommand Command { get; set; }
        public bool IsGrounded => groundContact != null && groundContact.IsGrounded;
        public bool IsRunning { get; private set; }
        public float ThrustFraction => mainRotor == null ? 0f : mainRotor.Thrust / _settings.maximumThrust;
        public Vector3 Velocity => body.linearVelocity;

        private void Awake() => Initialize();

        private void Initialize()
        {
            body = GetComponent<Rigidbody>();
            body.mass = _settings.mass;
            body.centerOfMass = _settings.centerOfMass;
            body.useGravity = true;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            mainRotor = new MainRotorHandler(body, _settings);
            handlers = new FlightHandler[]
            {
                mainRotor,
                new StabilizationHandler(body, _settings),
                new YawHandler(body, _settings),
                new AerodynamicDragHandler(body, _settings)
            };
            spawnPosition = body.position;
            spawnRotation = body.rotation;
        }

        private void FixedUpdate() => Step(Time.fixedDeltaTime);

        private void Step(float dt)
        {
            if (Command.Vertical > 0.01f) 
                IsRunning = true;
            else if (IsGrounded && Command.Vertical < -0.01f) 
                IsRunning = false;

            FlightFrame frame = new(Command, IsRunning, dt);
            foreach (FlightHandler handler in handlers)
            {
                if (handler.Enabled)
                    handler.Step(ref frame);   
            }
        }

        public void SetHandlerEnabled<T>(bool active) where T : FlightHandler
        {
            foreach (FlightHandler handler in handlers)
            {
                if (handler is not T) 
                    continue;
                
                handler.Enabled = active;
                return;
            }
        }

        public void ResetFlight()
        {
            body.position = spawnPosition;
            body.rotation = spawnRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Command = default;
            IsRunning = false;
            foreach (FlightHandler handler in handlers)
                handler.Reset();

            groundContact.Reset();
            body.WakeUp();
        }

        private void OnCollisionEnter(Collision collision) => groundContact.Enter(collision);
        private void OnCollisionStay(Collision collision) => groundContact.Stay(collision);
        private void OnCollisionExit(Collision collision) => groundContact.Exit(collision);
    }
}
