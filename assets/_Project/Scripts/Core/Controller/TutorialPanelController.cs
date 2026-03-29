using UnityEngine;

namespace _Project.Scripts.Core.Controller
{
    public class TutorialPanelController : UIBase
    {
        [SerializeField] private CanvasGroup tutorialCanvasGroup;

        protected override CanvasGroup TargetCanvasGroup => tutorialCanvasGroup;

        private void Awake()
        {
            Show();
        }

        public void HideTutorial()
        {
            Hide();
        }
    }
}
