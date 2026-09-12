using System;
using UnityEngine;

namespace Runtime.Presentation
{
    [Serializable]
    public sealed class RectTransformProgressBar
    {
        [SerializeField] private RectTransform _fill;

        public void SetValue(float value)
        {
            Vector2 anchorMax = _fill.anchorMax;
            anchorMax.x = Mathf.Clamp01(value);
            _fill.anchorMax = anchorMax;
        }
    }
}
