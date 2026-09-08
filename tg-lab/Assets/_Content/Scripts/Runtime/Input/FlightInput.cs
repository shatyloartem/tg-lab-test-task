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
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private HelicopterController helicopter;
        private InputActionAsset instance;
        private InputAction move;
        private InputAction vertical;
        private InputAction yaw;
        private InputAction reset;

        private void Awake()
        {
            // Each helicopter owns its action state, even when sharing the same asset.
            instance = Instantiate(actions);
            move = instance.FindAction("Flight/Move", true);
            vertical = instance.FindAction("Flight/Vertical", true);
            yaw = instance.FindAction("Flight/Yaw", true);
            reset = instance.FindAction("Flight/Reset", true);
        }

        private void OnEnable() => instance.Enable();

        private void Update()
        {
            if (reset.WasPressedThisFrame()) helicopter.ResetFlight();
            helicopter.Command = new FlightCommand(move.ReadValue<Vector2>(),
                vertical.ReadValue<float>(), yaw.ReadValue<float>());
        }

        private void OnDisable()
        {
            instance.Disable();
            helicopter.Command = default;
        }

        private void OnDestroy() => Destroy(instance);
    }
}
