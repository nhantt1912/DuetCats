using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectSongController : UIBase
{
    [SerializeField] private CanvasGroup _selectSongCanvasGroup;
    [SerializeField] private Button _buttonSongLeft;
    [SerializeField] private Button _buttonSongRight;

    public Action OnSelectSong;
    
    protected override CanvasGroup TargetCanvasGroup => _selectSongCanvasGroup;

    private void Awake()
    {
        _buttonSongLeft?.onClick.AddListener(()=> { OnSelectSong?.Invoke(); });
        _buttonSongRight?.onClick.AddListener(()=> { OnSelectSong?.Invoke(); });
    }
}
