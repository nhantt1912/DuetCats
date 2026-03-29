using UnityEngine;

public class SelectSongController : UIBase
{
    [SerializeField] private CanvasGroup selectSongCanvasGroup;

    protected override CanvasGroup TargetCanvasGroup => selectSongCanvasGroup;
}
