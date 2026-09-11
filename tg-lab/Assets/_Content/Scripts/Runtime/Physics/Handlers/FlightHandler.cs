using Runtime.Configuration;
using UnityEngine;

namespace Runtime.Physics.Handlers
{
    public abstract class FlightHandler
    {
        protected readonly Rigidbody Body;
        protected readonly HelicopterSettings Settings;

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                Reset();
            }
        }

        protected FlightHandler(Rigidbody body, HelicopterSettings settings)
        {
            Body = body;
            Settings = settings;
        }

        public abstract void Step(ref FlightFrame frame);

        public virtual void Reset()
        { }
    }
}
