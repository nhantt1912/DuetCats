using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core.Controller;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private TileManager _tileManager;
    [SerializeField] private CatManager _catManager;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private TutorialPanelController _tutorialPanelController;
    [SerializeField] private SelectSongController _selectSongController;
    [Header("Game Rules")]
    [SerializeField] private float hitToleranceX = 0.8f; // X-distance tolerance for a successful hit

    private int _eatenTileCount;
    private bool _hasTriggeredWin;

    private void Awake()
    {
        _tileManager.OnTileHitZone += HandleTileReachHitZone;
        _tileManager.OnLose += HandleLose;

        if (_catManager != null)
        {
            _catManager.OnWinAnimationFinished += HandleWinAnimationFinished;
        }
    }

    private void OnDestroy()
    {
        if (_tileManager != null)
        {
            _tileManager.OnTileHitZone -= HandleTileReachHitZone;
            _tileManager.OnLose -= HandleLose;
        }

        if (_catManager != null)
        {
            _catManager.OnWinAnimationFinished -= HandleWinAnimationFinished;
        }
    }

    private void Update()
    {
        if (_tileManager == null || !_tileManager.IsWaitingForStartTap)
        {
            return;
        }

        if (TryConsumeStartTap())
        {
            _tutorialPanelController?.HideTutorial();
            _tileManager.BeginGameplay();
        }
    }

    private void HandleTileReachHitZone(Tile tile)
    {
        bool isHit = _catManager.TryEatTile(tile, hitToleranceX);

        if (isHit)
        {
            if (_scoreManager != null)
            {
                _scoreManager.AddScoreForTile(tile);
            }

            _tileManager.Release(tile);

            _eatenTileCount++;
            TryTriggerWin();
        }
        else
        {
          Debug.Log("Not Hit");
        }
    }

    private void HandleLose(Tile failedTile)
    {
        _catManager.PlayLoseAnimation(failedTile);
    }

    private void TryTriggerWin()
    {
        if (_hasTriggeredWin || _tileManager == null || _catManager == null)
        {
            return;
        }

        if (_eatenTileCount < _tileManager.TotalTileCount)
        {
            return;
        }

        _hasTriggeredWin = true;
        _catManager.PlayWinAnimation();
    }

    private void HandleWinAnimationFinished()
    {
        _selectSongController.Show();
    }

    private static bool TryConsumeStartTap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        if (Input.touchCount <= 0)
        {
            return false;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                return true;
            }
        }

        return false;
    }


}
