using System.Reflection;
using UnityEngine;

namespace _Project.Scripts.Core.Adaptive.Adapters
{
    [DisallowMultipleComponent]
    public class InputViewportAdapter : MonoBehaviour
    {
        [SerializeField] private DualInputHandler _dualInputHandler;

        private static readonly BindingFlags _fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static FieldInfo _midXField;
        private static FieldInfo _halfWidthField;

        public void Configure(DualInputHandler inputHandler)
        {
            _dualInputHandler = inputHandler;
        }

        [ContextMenu("Refresh Viewport Metrics")]
        public void RefreshViewportMetrics()
        {
            if (_dualInputHandler == null)
            {
                return;
            }

            CacheFields();

            float halfWidth = Screen.width * 0.5f;
            _midXField?.SetValue(_dualInputHandler, halfWidth);
            _halfWidthField?.SetValue(_dualInputHandler, halfWidth);
        }

        private void CacheFields()
        {
            if (_midXField != null)
            {
                return;
            }

            System.Type inputType = typeof(DualInputHandler);
            _midXField = inputType.GetField("_midX", _fieldFlags);
            _halfWidthField = inputType.GetField("_halfWidth", _fieldFlags);
        }
    }
}




