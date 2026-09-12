using System;
using TMPro;
using UnityEngine;

namespace Runtime.Presentation
{
    [Serializable]
    public sealed class FlightAxisIndicator
    {
        [SerializeField] private RectTransform _negativeFill;
        [SerializeField] private RectTransform _positiveFill;
        [SerializeField] private TMP_Text _value;

        public void SetValue(float value)
        {
            value = Mathf.Clamp(value, -1f, 1f);

            _negativeFill.anchorMin = new Vector2(0.5f + Mathf.Min(value, 0f) * 0.5f, 0f);
            _negativeFill.anchorMax = new Vector2(0.5f, 1f);
            _positiveFill.anchorMin = new Vector2(0.5f, 0f);
            _positiveFill.anchorMax = new Vector2(0.5f + Mathf.Max(value, 0f) * 0.5f, 1f);
            _value.text = value.ToString("+0.00;-0.00;0.00");
        }
    }
}
