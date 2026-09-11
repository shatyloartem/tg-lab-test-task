using Runtime.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Input
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

    public sealed class FlightInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private HelicopterController _helicopter;
        
        private InputActionAsset _instance;
        private InputAction _move;
        private InputAction _vertical;
        private InputAction _yaw;
        private InputAction _reset;

        private void Awake()
        {
            _instance = Instantiate(_actions);

            _move = GetAction("Flight/Move");
            _vertical = GetAction("Flight/Vertical");
            _yaw = GetAction("Flight/Yaw");
            _reset = GetAction("Flight/Reset");
            return;

            InputAction GetAction(string a) => _instance.FindAction(a, true);
        }

        private void OnEnable() => _instance.Enable();

        private void Update()
        {
            if (_reset.WasPressedThisFrame()) 
                _helicopter.ResetFlight();
            
            _helicopter.Command = new FlightCommand(
                _move.ReadValue<Vector2>(), 
                _vertical.ReadValue<float>(), 
                _yaw.ReadValue<float>());
        }

        private void OnDisable()
        {
            _instance.Disable();
            _helicopter.Command = default;
        }

        private void OnDestroy() => Destroy(_instance);
    }
}
