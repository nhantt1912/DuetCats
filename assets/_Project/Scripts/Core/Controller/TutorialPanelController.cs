using UnityEngine;

namespace _Project.Scripts.Core.Controller
{
    public class TutorialPanelController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup tutorialCanvasGroup;
        [SerializeField] private float fadeDuration = 0.25f;

        private bool _isHidden;
        private bool _isFading;
        private float _fadeElapsed;

        private void Awake()
        {
            if (tutorialCanvasGroup == null)
            {
                return;
            }

            tutorialCanvasGroup.alpha = 1f;
        }

        private void Update()
        {
            if (tutorialCanvasGroup == null || _isHidden || !_isFading)
            {
                return;
            }

            _fadeElapsed += Time.deltaTime;
            float duration = Mathf.Max(0.01f, fadeDuration);
            float t = Mathf.Clamp01(_fadeElapsed / duration);
            tutorialCanvasGroup.alpha = 1f - t;

            if (tutorialCanvasGroup.alpha <= 0f)
            {
                _isHidden = true;
                _isFading = false;
            }
        }

        public void HideTutorial()
        {
            if (tutorialCanvasGroup == null || _isHidden || _isFading)
            {
                return;
            }

             _fadeElapsed = 0f;
             _isFading = true;
        }

    }
}
