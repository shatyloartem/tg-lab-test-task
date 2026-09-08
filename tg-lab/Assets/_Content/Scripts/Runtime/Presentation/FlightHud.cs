using Runtime.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    public sealed class FlightHud : MonoBehaviour
    {
        [SerializeField] private HelicopterController helicopter;
        [SerializeField] private Text altitude;
        [SerializeField] private Text speed;
        [SerializeField] private Text verticalSpeed;
        [SerializeField] private Text status;
        [SerializeField] private Image thrustBar;
        private float referenceHeight;

        private void Start() => referenceHeight = helicopter.transform.position.y;

        private void Update()
        {
            altitude.text = Mathf.Max(0f, helicopter.transform.position.y - referenceHeight).ToString("0.0");
            speed.text = (Vector3.ProjectOnPlane(helicopter.Velocity, Vector3.up).magnitude * 3.6f).ToString("0");
            verticalSpeed.text = helicopter.Velocity.y.ToString("+0.0;-0.0;0.0");
            status.text = helicopter.IsGrounded ? (helicopter.IsRunning ? "TAKING OFF" : "READY TO FLY") :
                helicopter.IsRunning ? "IN FLIGHT" : "ROTOR OFF";
            thrustBar.fillAmount = helicopter.ThrustFraction;
        }
    }
}
