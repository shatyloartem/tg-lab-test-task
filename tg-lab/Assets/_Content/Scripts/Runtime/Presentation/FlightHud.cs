using Runtime.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    public sealed class FlightHud : MonoBehaviour
    {
        [FormerlySerializedAs("helicopter")]
        [SerializeField] private HelicopterController _helicopter;

        [FormerlySerializedAs("altitude")]
        [SerializeField] private TMP_Text _altitude;

        [FormerlySerializedAs("speed")]
        [SerializeField] private TMP_Text _speed;

        [FormerlySerializedAs("verticalSpeed")]
        [SerializeField] private TMP_Text _verticalSpeed;

        [FormerlySerializedAs("status")]
        [SerializeField] private TMP_Text _status;

        [FormerlySerializedAs("thrustBar")]
        [SerializeField] private Image _thrustBar;

        private float _referenceHeight;

        private void Start() => _referenceHeight = _helicopter.transform.position.y;

        private void Update()
        {
            _altitude.text = Mathf.Max(0f, _helicopter.transform.position.y - _referenceHeight).ToString("0.0");
            _speed.text = (Vector3.ProjectOnPlane(_helicopter.Velocity, Vector3.up).magnitude * 3.6f).ToString("0");
            _verticalSpeed.text = _helicopter.Velocity.y.ToString("+0.0;-0.0;0.0");
            _thrustBar.fillAmount = _helicopter.ThrustFraction;
        }
    }
}
