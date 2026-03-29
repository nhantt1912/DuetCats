using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected abstract CanvasGroup TargetCanvasGroup { get; }

    public virtual void Show()
    {
        SetVisible(true);
    }

    public virtual void Hide()
    {
        SetVisible(false);
    }

    protected void SetVisible(bool isVisible)
    {
        CanvasGroup canvasGroup = TargetCanvasGroup;
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
}

