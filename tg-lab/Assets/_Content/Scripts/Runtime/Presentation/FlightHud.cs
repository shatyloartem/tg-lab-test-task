using Runtime.Controllers;
using Runtime.Physics;
using TMPro;
using UnityEngine;

namespace Runtime.Presentation
{
    public sealed class FlightHud : MonoBehaviour
    {
        [SerializeField] private HelicopterController _helicopter;

        [Header("Telemetry")]
        [SerializeField] private TMP_Text _altitude;
        [SerializeField] private TMP_Text _speed;
        [SerializeField] private TMP_Text _verticalSpeed;
        [SerializeField] private TMP_Text _rotorReadout;
        [SerializeField] private RectTransformProgressBar _thrustBar;

        [Header("Control axes")]
        [SerializeField] private FlightAxisIndicator _verticalInput;
        [SerializeField] private FlightAxisIndicator _pitchInput;
        [SerializeField] private FlightAxisIndicator _rollInput;
        [SerializeField] private FlightAxisIndicator _yawInput;

        [Header("Flight assists")]
        [SerializeField] private FlightAssistToggleBinding[] _flightAssists;

        private float _referenceHeight;

        private void Start()
        {
            _referenceHeight = _helicopter.transform.position.y;
            _thrustBar.SetValue(0f);

            foreach (var assist in _flightAssists)
                assist.Bind(_helicopter);
        }

        private void Update()
        {
            _altitude.text = Mathf.Max(0f, _helicopter.transform.position.y - _referenceHeight).ToString("0.0");
            _speed.text = (Vector3.ProjectOnPlane(_helicopter.Velocity, Vector3.up).magnitude * 3.6f).ToString("0");
            _verticalSpeed.text = _helicopter.Velocity.y.ToString("+0.0;-0.0;0.0");
            _thrustBar.SetValue(_helicopter.ThrustFraction);

            FlightCommand command = _helicopter.CurrentCommand;
            _verticalInput.SetValue(command.Vertical);
            _pitchInput.SetValue(command.Cyclic.y);
            _rollInput.SetValue(command.Cyclic.x);
            _yawInput.SetValue(command.Yaw);

            _rotorReadout.text = $"MAIN ROTOR  {_helicopter.MainRotorThrust / 1000f:0.0} kN  /  {_helicopter.ThrustFraction * 100f:0}%";
        }

        private void OnDestroy()
        {
            foreach (var assist in _flightAssists)
                assist.Unbind();
        }
    }
}
