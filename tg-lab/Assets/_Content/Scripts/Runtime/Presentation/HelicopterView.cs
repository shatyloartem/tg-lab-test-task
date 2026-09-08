using Runtime.Controllers;
using UnityEngine;

namespace Runtime.Presentation
{
    public sealed class HelicopterView : MonoBehaviour
    {
        [SerializeField] private HelicopterController helicopter;
        [SerializeField] private Transform mainRotor;
        [SerializeField] private Transform tailRotor;
        private float rotorSpeed;

        private void Update()
        {
            float target = helicopter.IsRunning ? 950f + helicopter.ThrustFraction * 650f : 0f;
            rotorSpeed = Mathf.MoveTowards(rotorSpeed, target, 900f * Time.deltaTime);
            mainRotor.Rotate(Vector3.up, rotorSpeed * Time.deltaTime, Space.Self);
            tailRotor.Rotate(Vector3.right, rotorSpeed * 1.6f * Time.deltaTime, Space.Self);
        }
    }
}
