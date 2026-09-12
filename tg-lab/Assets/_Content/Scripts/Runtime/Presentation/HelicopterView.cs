using Runtime.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Presentation
{
    public sealed class HelicopterView : MonoBehaviour
    {
        [FormerlySerializedAs("helicopter")]
        [SerializeField] private HelicopterController _helicopter;

        [FormerlySerializedAs("mainRotor")]
        [SerializeField] private Transform _mainRotor;

        [FormerlySerializedAs("tailRotor")]
        [SerializeField] private Transform _tailRotor;

        private float _rotorSpeed;

        private void Update()
        {
            float target = _helicopter.IsRunning ? 950f + _helicopter.ThrustFraction * 650f : 0f;
            _rotorSpeed = Mathf.MoveTowards(_rotorSpeed, target, 900f * Time.deltaTime);
            _mainRotor.Rotate(Vector3.up, _rotorSpeed * Time.deltaTime, Space.Self);
            _tailRotor.Rotate(Vector3.right, _rotorSpeed * 1.6f * Time.deltaTime, Space.Self);
        }
    }
}
