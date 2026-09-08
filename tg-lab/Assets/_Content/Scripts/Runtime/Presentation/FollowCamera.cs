using UnityEngine;

namespace Runtime.Presentation
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(8f, 6f, -12f);
        [SerializeField, Min(0.1f)] private float response = 4f;
        private bool initialized;

        private void LateUpdate()
        {
            Vector3 focus = target.position + Vector3.up;
            Vector3 desired = focus + Quaternion.Euler(0f, target.eulerAngles.y, 0f) * offset;
            Vector3 travel = desired - focus;
            if (UnityEngine.Physics.SphereCast(focus, 0.3f, travel.normalized, out RaycastHit hit,
                travel.magnitude, UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                desired = focus + travel.normalized * Mathf.Max(0.1f, hit.distance - 0.15f);

            transform.position = initialized && Vector3.Distance(transform.position, desired) < 50f
                ? Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-response * Time.deltaTime))
                : desired;
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
            initialized = true;
        }
    }
}
