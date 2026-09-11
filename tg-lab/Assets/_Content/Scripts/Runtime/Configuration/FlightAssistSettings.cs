using System;
using UnityEngine;

namespace Runtime.Configuration
{
    [Flags]
    public enum FlightAssistFeature
    {
        None = 0,
        CyclicInputFiltering = 1 << 0,
        AttitudeStabilization = 1 << 1,
        AngularVelocityDamping = 1 << 2,
        AltitudeHold = 1 << 3,
        HeadingHold = 1 << 4,
        MainRotorTorqueCompensation = 1 << 5
    }

    [Serializable]
    public sealed class FlightAssistSettings
    {
        [SerializeField] private FlightAssistFeature _enabledFeatures = ~FlightAssistFeature.None;

        public FlightAssistFeature EnabledFeatures
        {
            get => _enabledFeatures;
            set => _enabledFeatures = value;
        }

        public bool IsEnabled(FlightAssistFeature feature) =>
            feature != FlightAssistFeature.None && (_enabledFeatures & feature) == feature;

        public void SetEnabled(FlightAssistFeature feature, bool enabled)
        {
            if (enabled)
                _enabledFeatures |= feature;
            else
                _enabledFeatures &= ~feature;
        }
    }
}
