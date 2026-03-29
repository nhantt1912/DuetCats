using System;
using UnityEngine;

namespace _Project.Scripts.Core.Adaptive
{
    [DisallowMultipleComponent]
    public class AdaptiveCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _targetCamera;

        [Header("Detection")]
        [SerializeField, Min(1f)] private float _checkInterval = 0.1f;
        [SerializeField, Range(0.9f, 1.2f)] private float _landscapeThreshold = 1.05f;
        [SerializeField, Range(0.8f, 1.1f)] private float _portraitThreshold = 0.95f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = true;
        [SerializeField] private bool _logModeChanges = true;
        [SerializeField] private string _debugModeLabel = "Unknown";

        public event Action<AdaptiveViewMode> OnViewModeChanged;

        public AdaptiveViewMode CurrentMode { get; private set; } = AdaptiveViewMode.Portrait;

        private float _nextCheckTime;

        private void Reset()
        {
            _targetCamera = GetComponent<Camera>();
        }

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = GetComponent<Camera>();
            }

            ForceRefresh();
        }

        private void OnEnable()
        {
            ForceRefresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.unscaledTime + _checkInterval;
            EvaluateMode();
        }

        public void Configure(Camera cameraRef, bool enableDebug = true, bool enableLogs = true)
        {
            _targetCamera = cameraRef;
            _debugMode = enableDebug;
            _logModeChanges = enableLogs;
            ForceRefresh();
        }

        [ContextMenu("Force Refresh Mode")]
        public void ForceRefresh()
        {
            _nextCheckTime = Time.unscaledTime;
            EvaluateMode(forceNotify: true);
        }

        public string GetDebugModeLabel()
        {
            return _debugModeLabel;
        }

        private void EvaluateMode(bool forceNotify = false)
        {
            float aspect = GetAspect();
            AdaptiveViewMode nextMode = ResolveModeFromAspect(aspect, CurrentMode);

            bool hasChanged = nextMode != CurrentMode;
            if (!hasChanged && !forceNotify)
            {
                UpdateDebugLabel(CurrentMode, aspect, false);
                return;
            }

            CurrentMode = nextMode;
            UpdateDebugLabel(CurrentMode, aspect, true);

            if (_logModeChanges && (hasChanged || forceNotify))
            {
                Debug.Log($"[AdaptiveCameraController] Mode={CurrentMode}, aspect={aspect:F3}, width={Screen.width}, height={Screen.height}");
            }

            OnViewModeChanged?.Invoke(CurrentMode);
        }

        private AdaptiveViewMode ResolveModeFromAspect(float aspect, AdaptiveViewMode current)
        {
            if (current == AdaptiveViewMode.Portrait)
            {
                return aspect > _landscapeThreshold ? AdaptiveViewMode.Landscape : AdaptiveViewMode.Portrait;
            }

            return aspect < _portraitThreshold ? AdaptiveViewMode.Portrait : AdaptiveViewMode.Landscape;
        }

        private float GetAspect()
        {
            if (_targetCamera != null)
            {
                Rect pixelRect = _targetCamera.pixelRect;
                if (pixelRect.height > 0.001f)
                {
                    return pixelRect.width / pixelRect.height;
                }
            }

            return Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
        }

        private void UpdateDebugLabel(AdaptiveViewMode mode, float aspect, bool includeAspect)
        {
            if (!_debugMode)
            {
                _debugModeLabel = "Debug Disabled";
                return;
            }

            _debugModeLabel = includeAspect ? $"{mode} (aspect {aspect:F3})" : mode.ToString();
        }
    }
}



