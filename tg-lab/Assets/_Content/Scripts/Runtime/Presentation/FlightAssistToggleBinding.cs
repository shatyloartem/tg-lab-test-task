using System;
using Runtime.Configuration;
using Runtime.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Presentation
{
    [Serializable]
    public sealed class FlightAssistToggleBinding
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private FlightAssistFeature _feature;

        private HelicopterController _helicopter;

        public void Bind(HelicopterController helicopter)
        {
            _helicopter = helicopter;
            _toggle.SetIsOnWithoutNotify(_helicopter.IsFlightAssistEnabled(_feature));
            _toggle.onValueChanged.AddListener(SetEnabled);
        }

        public void Unbind()
        {
            _toggle.onValueChanged.RemoveListener(SetEnabled);
            _helicopter = null;
        }

        private void SetEnabled(bool enabled) => _helicopter.SetFlightAssistEnabled(_feature, enabled);
    }
}
