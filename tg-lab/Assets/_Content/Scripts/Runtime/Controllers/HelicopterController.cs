using Runtime.Configuration;
using Runtime.Physics;
using Runtime.Physics.Handlers;
using UnityEngine;

namespace Runtime.Controllers
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class HelicopterController : MonoBehaviour
    {
        [SerializeField] private HelicopterSettings _settings;

        private FlightHandler[] _flightPipeline;
        private MainRotorHandler _mainRotorHandler;
        private readonly GroundContactHandler r_groundContactHandler = new();
        
        private Rigidbody _body;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private FlightCommand _command;
        private bool _resetRequested;

        public bool IsGrounded => r_groundContactHandler.IsGrounded;
        public bool IsRunning { get; private set; }
        public float ThrustFraction => _mainRotorHandler == null ? 0f : _mainRotorHandler.Thrust / _settings._maximumThrust;
        public Vector3 Velocity => _body.linearVelocity;

        private void Awake() => Initialize();

        private void Initialize()
        {
            SetupRigidbody();

            _mainRotorHandler = new MainRotorHandler(_body, _settings);
            _flightPipeline = new FlightHandler[]
            {
                // Control phase
                new PilotControlHandler(_body, _settings),
                new StabilizationHandler(_body, _settings),

                // Physics phase
                _mainRotorHandler,
                new CyclicHandler(_body, _settings),
                new TailRotorHandler(_body, _settings),
                new AerodynamicDragHandler(_body, _settings)
            };

            _spawnPosition = _body.position;
            _spawnRotation = _body.rotation;
        }

        private void FixedUpdate()
        {
            if (_resetRequested)
                ResetFlight();

            Step(Time.fixedDeltaTime);
        }

        private void Step(float dt)
        {
            UpdateRunningState();

            FlightFrame frame = new(_command, IsRunning, dt);
            foreach (var handler in _flightPipeline)
            {
                if (handler.Enabled)
                    handler.Step(ref frame);
            }
        }

        public void SetCommand(FlightCommand command) => _command = command;

        public void RequestReset() => _resetRequested = true;

        private void ResetFlight()
        {
            _body.position = _spawnPosition;
            _body.rotation = _spawnRotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            _command = default;
            _resetRequested = false;
            IsRunning = false;
            
            foreach (var handler in _flightPipeline)
                handler.Reset();

            r_groundContactHandler.Reset();
            _body.WakeUp();
        }

        public void SetHandlerEnabled<T>(bool active) where T : FlightHandler
        {
            foreach (var handler in _flightPipeline)
            {
                if (handler is T)
                {
                    handler.Enabled = active;
                    return;
                }
            }
        }
        
        private void SetupRigidbody()
        {
            _body = GetComponent<Rigidbody>();
            _body.mass = _settings._mass;
            _body.centerOfMass = _settings._centerOfMass;
            _body.useGravity = true;
            _body.linearDamping = 0f;
            _body.angularDamping = 0f;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void UpdateRunningState()
        {
            if (_command.Vertical > 0.01f)
                IsRunning = true;
            else if (IsGrounded && _command.Vertical < -0.01f)
                IsRunning = false;
        }
        
        private void OnCollisionEnter(Collision collision) => r_groundContactHandler.Enter(collision);
        private void OnCollisionStay(Collision collision) => r_groundContactHandler.Stay(collision);
        private void OnCollisionExit(Collision collision) => r_groundContactHandler.Exit(collision);
    }
}
