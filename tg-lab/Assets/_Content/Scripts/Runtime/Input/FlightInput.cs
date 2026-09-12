using Runtime.Controllers;
using Runtime.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Input
{
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
                _helicopter.RequestReset();
            
            _helicopter.SetCommand(new FlightCommand(
                _move.ReadValue<Vector2>(), 
                _vertical.ReadValue<float>(), 
                _yaw.ReadValue<float>()));
        }

        private void OnDisable()
        {
            _instance.Disable();
            _helicopter.SetCommand(default);
        }

        private void OnDestroy() => Destroy(_instance);
    }
}
