using UnityEngine;
using _Project.Scripts.Core.Adaptive.Adapters;

namespace _Project.Scripts.Core.Adaptive
{
    [DisallowMultipleComponent]
    public class AdaptiveViewOrchestrator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AdaptiveCameraController _adaptiveCameraController;
        [SerializeField] private AdaptiveViewProfile _adaptiveViewProfile;
        [SerializeField] private Camera _targetCamera;

        [Header("Backgrounds")]
        [SerializeField] private GameObject _portraitBackground;
        [SerializeField] private GameObject _landscapeBackground;
        [SerializeField] private bool _autoFitBackgroundToViewport = true;
        [SerializeField, Min(1f)] private float _backgroundOverscan = 1.01f;

        [Header("Adapters")]
        [SerializeField] private TileLayoutAdapter _tileLayoutAdapter;
        [SerializeField] private CatBoundsAdapter _leftCatBoundsAdapter;
        [SerializeField] private CatBoundsAdapter _rightCatBoundsAdapter;
        [SerializeField] private InputViewportAdapter _inputViewportAdapter;

        private bool _isSubscribed;
        private bool _portraitScaleCached;
        private bool _landscapeScaleCached;
        private Vector3 _portraitBaseScale = Vector3.one;
        private Vector3 _landscapeBaseScale = Vector3.one;

        private void Awake()
        {
            if (_adaptiveCameraController == null)
            {
                _adaptiveCameraController = GetComponent<AdaptiveCameraController>();
            }

            if (_targetCamera == null)
            {
                _targetCamera = GetComponent<Camera>();
            }
        }

        private void OnEnable()
        {
            BindAndApply();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Configure(
            AdaptiveCameraController cameraController,
            AdaptiveViewProfile viewProfile,
            Camera cameraRef,
            GameObject portraitBg,
            GameObject landscapeBg,
            TileLayoutAdapter tileAdapter,
            CatBoundsAdapter leftAdapter,
            CatBoundsAdapter rightAdapter,
            InputViewportAdapter viewportAdapter)
        {
            _adaptiveCameraController = cameraController;
            _adaptiveViewProfile = viewProfile;
            _targetCamera = cameraRef;
            _portraitBackground = portraitBg;
            _landscapeBackground = landscapeBg;
            _tileLayoutAdapter = tileAdapter;
            _leftCatBoundsAdapter = leftAdapter;
            _rightCatBoundsAdapter = rightAdapter;
            _inputViewportAdapter = viewportAdapter;

            BindAndApply();
        }

        public void ApplyCurrentMode()
        {
            if (_adaptiveCameraController != null)
            {
                ApplyMode(_adaptiveCameraController.CurrentMode);
            }
        }

        private void BindAndApply()
        {
            if (_adaptiveCameraController == null)
            {
                return;
            }

            if (!_isSubscribed)
            {
                _adaptiveCameraController.OnViewModeChanged += ApplyMode;
                _isSubscribed = true;
            }

            ApplyMode(_adaptiveCameraController.CurrentMode);
        }

        private void Unbind()
        {
            if (!_isSubscribed || _adaptiveCameraController == null)
            {
                return;
            }

            _adaptiveCameraController.OnViewModeChanged -= ApplyMode;
            _isSubscribed = false;
        }

        private void ApplyMode(AdaptiveViewMode mode)
        {
            ApplyCamera(mode);
            ApplyBackgrounds(mode);
            ApplyGameplayLayout(mode);
        }

        private void ApplyBackgrounds(AdaptiveViewMode mode)
        {
            bool isLandscape = mode == AdaptiveViewMode.Landscape;

            if (_portraitBackground != null)
            {
                _portraitBackground.SetActive(!isLandscape);
            }

            if (_landscapeBackground != null)
            {
                _landscapeBackground.SetActive(isLandscape);
            }

            if (!_autoFitBackgroundToViewport)
            {
                return;
            }

            FitBackgroundToViewport(_portraitBackground, ref _portraitScaleCached, ref _portraitBaseScale);
            FitBackgroundToViewport(_landscapeBackground, ref _landscapeScaleCached, ref _landscapeBaseScale);
        }

        private void ApplyCamera(AdaptiveViewMode mode)
        {
            if (_adaptiveViewProfile == null || _targetCamera == null)
            {
                return;
            }

            AdaptiveViewProfile.ModeCameraSettings cameraSettings = _adaptiveViewProfile.GetCamera(mode);
            _targetCamera.orthographicSize = cameraSettings.orthographicSize;
            _targetCamera.transform.localPosition = cameraSettings.localPosition;
        }

        private void FitBackgroundToViewport(GameObject background, ref bool hasCachedScale, ref Vector3 baseScale)
        {
            if (background == null || _targetCamera == null || !_targetCamera.orthographic)
            {
                return;
            }

            SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Transform bgTransform = background.transform;
            if (!hasCachedScale)
            {
                baseScale = bgTransform.localScale;
                hasCachedScale = true;
            }

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= 0.0001f || spriteSize.y <= 0.0001f)
            {
                return;
            }

            float viewportHeight = _targetCamera.orthographicSize * 2f;
            float viewportWidth = viewportHeight * _targetCamera.aspect;

            float scaleX = viewportWidth / spriteSize.x;
            float scaleY = viewportHeight / spriteSize.y;
            float coverScale = Mathf.Max(scaleX, scaleY) * _backgroundOverscan;

            bgTransform.localScale = new Vector3(
                baseScale.x * coverScale,
                baseScale.y * coverScale,
                baseScale.z);
        }

        private void ApplyGameplayLayout(AdaptiveViewMode mode)
        {
            if (_adaptiveViewProfile != null)
            {
                AdaptiveViewProfile.ModeTileSettings tiles = _adaptiveViewProfile.GetTiles(mode);
                _tileLayoutAdapter?.Apply(tiles);

                AdaptiveViewProfile.ModeCatBounds leftBounds = _adaptiveViewProfile.GetLeftCatBounds(mode);
                AdaptiveViewProfile.ModeCatBounds rightBounds = _adaptiveViewProfile.GetRightCatBounds(mode);
                _leftCatBoundsAdapter?.Apply(leftBounds);
                _rightCatBoundsAdapter?.Apply(rightBounds);
            }

            _inputViewportAdapter?.RefreshViewportMetrics();
        }
    }
}




