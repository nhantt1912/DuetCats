using UnityEngine;

namespace _Project.Scripts.Core.Controller
{
    public class TutorialPanelController : UIBase
    {
        [SerializeField] private CanvasGroup _tutorialCanvasGroup;

        protected override CanvasGroup TargetCanvasGroup => _tutorialCanvasGroup;

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
