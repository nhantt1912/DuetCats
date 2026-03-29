using System.Collections;
using UnityEngine;
using _Project.Scripts.Core.Controller;

public class GameplayController : MonoBehaviour
{
    private enum GameplayState
    {
        WaitingForStart,
        Playing,
        PendingWin,
        PendingLose,
        WinAnimating,
        Lost,
        Completed
    }

    [SerializeField] private TileManager _tileManager;
    [SerializeField] private CatManager _catManager;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private TutorialPanelController _tutorialPanelController;
    [SerializeField] private SelectSongController _selectSongController;
    [Header("Game Rules")]
    [SerializeField] private float _hitToleranceX = 0.8f; // X-distance tolerance for a successful hit
    [SerializeField] private float _winPopupDelaySeconds = 2f;
    [SerializeField] private float _winAnimationDelaySeconds = 1f;

    [Header("Lose Flow")]
    [SerializeField] private float _loseRestartDelaySeconds = 5f;

    private int _eatenTileCount;
    private GameplayState _state = GameplayState.WaitingForStart;
    private Coroutine _showSelectSongCoroutine;
    private Coroutine _pendingOutcomeCoroutine;
    private Coroutine _restartAfterLoseCoroutine;
    private bool _canStartPendingWinAnimation;

    private void Awake()
    {
        _tileManager.OnTileHitZone += HandleTileReachHitZone;
        _tileManager.OnLose += HandleLose;
        _selectSongController.OnSelectSong += OnSelectSong;

        if (_catManager != null)
        {
            _catManager.OnAnyCatEatingStateChanged += HandleAnyCatEatingStateChanged;
        }

        _state = (_tileManager != null && _tileManager.IsWaitingForStartTap)
            ? GameplayState.WaitingForStart
            : GameplayState.Playing;
        _canStartPendingWinAnimation = false;
    }

    private void OnSelectSong()
    {
        RestartRound(true);
    }

    private void OnDestroy()
    {
        if (_showSelectSongCoroutine != null)
        {
            StopCoroutine(_showSelectSongCoroutine);
            _showSelectSongCoroutine = null;
        }

        if (_pendingOutcomeCoroutine != null)
        {
            StopCoroutine(_pendingOutcomeCoroutine);
            _pendingOutcomeCoroutine = null;
        }

        if (_restartAfterLoseCoroutine != null)
        {
            StopCoroutine(_restartAfterLoseCoroutine);
            _restartAfterLoseCoroutine = null;
        }

        if (_tileManager != null)
        {
            _tileManager.OnTileHitZone -= HandleTileReachHitZone;
            _tileManager.OnLose -= HandleLose;
        }

        if (_catManager != null)
        {
            _catManager.OnAnyCatEatingStateChanged -= HandleAnyCatEatingStateChanged;
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
            _catManager?.PlayListeningAnimation();
            _tutorialPanelController?.HideTutorial();
            _tileManager.BeginGameplay();
            _state = GameplayState.Playing;
        }
    }

    private void HandleTileReachHitZone(Tile tile)
    {
        if (_state != GameplayState.Playing)
        {
            return;
        }

        bool isHit = _catManager.TryEatTile(tile, _hitToleranceX);

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

    private void HandleLose()
    {
        if (_state == GameplayState.WinAnimating || _state == GameplayState.Completed || _state == GameplayState.Lost || _state == GameplayState.PendingLose)
        {
            return;
        }

        BeginPendingOutcome(GameplayState.PendingLose);
    }

    

    private void TryTriggerWin()
    {
        if (_state != GameplayState.Playing || _tileManager == null || _catManager == null)
        {
            return;
        }

        if (_eatenTileCount < _tileManager.TotalTileCount)
        {
            return;
        }
    
        BeginPendingOutcome(GameplayState.PendingWin);
    }

    private void HandleAnyCatEatingStateChanged(bool anyCatEating)
    {
        if (!anyCatEating)
        {
            TryStartWinAnimation();
        }
    }

    private void BeginPendingOutcome(GameplayState pendingState)
    {
        _state = pendingState;
        _canStartPendingWinAnimation = false;

        if (_pendingOutcomeCoroutine != null)
        {
            StopCoroutine(_pendingOutcomeCoroutine);
        }

        _pendingOutcomeCoroutine = StartCoroutine(ResolvePendingOutcomeAfterDelay(pendingState));
    }

    private IEnumerator ResolvePendingOutcomeAfterDelay(GameplayState pendingState)
    {
        yield return new WaitForSeconds(_winAnimationDelaySeconds);

        _pendingOutcomeCoroutine = null;
        if (_state != pendingState)
        {
            yield break;
        }

        if (pendingState == GameplayState.PendingLose)
        {
            _state = GameplayState.Lost;
            _catManager?.PlayLoseAnimation();
            
            if (_restartAfterLoseCoroutine == null)
            {
                _restartAfterLoseCoroutine = StartCoroutine(RestartAfterLoseDelay());
            }
            yield break;
        }

        if (pendingState != GameplayState.PendingWin)
        {
            yield break;
        }

        _canStartPendingWinAnimation = true;
        TryStartWinAnimation();
    }

    private IEnumerator RestartAfterLoseDelay()
    {
        yield return new WaitForSecondsRealtime(_loseRestartDelaySeconds);

        _restartAfterLoseCoroutine = null;
        RestartRound(autoStart: true);
    }

    private void RestartRound(bool autoStart)
    {
        _eatenTileCount = 0;
        _canStartPendingWinAnimation = false;

        if (_showSelectSongCoroutine != null)
        {
            StopCoroutine(_showSelectSongCoroutine);
        }

        if (_pendingOutcomeCoroutine != null)
        {
            StopCoroutine(_pendingOutcomeCoroutine);
        }

        _showSelectSongCoroutine = null;
        _pendingOutcomeCoroutine = null;

        _selectSongController?.Hide();
        _scoreManager?.ResetScore();
        _catManager?.ResetCatsToIdle();

        if (_tileManager == null)
        {
            _state = GameplayState.WaitingForStart;
            return;
        }

        bool waitForTap = !autoStart;
        _tileManager.ResetGameplayState(waitForTap);

        if (autoStart)
        {
            _catManager?.PlayListeningAnimation();
            _tileManager.BeginGameplay();
            _state = GameplayState.Playing;
            return;
        }

        _state = GameplayState.WaitingForStart;
    }
    
    private void TryStartWinAnimation()
    {
        if (_state != GameplayState.PendingWin || _catManager == null || !_canStartPendingWinAnimation)
        {
            return;
        }

        if (_catManager.AreAnyCatsEating)
        {
            return;
        }

        Debug.Log("Trigger Win");
        _state = GameplayState.WinAnimating;
        _catManager.PlayAnimationCatWin();
        HandleWinAnimationFinished();
    }

    private void HandleWinAnimationFinished()
    {
        if (_state != GameplayState.WinAnimating)
        {
            return;
        }

        _state = GameplayState.Completed;

        if (_showSelectSongCoroutine != null)
        {
            StopCoroutine(_showSelectSongCoroutine);
        }

        _showSelectSongCoroutine = StartCoroutine(ShowSelectSongAfterDelay());
    }

    private IEnumerator ShowSelectSongAfterDelay()
    {
        yield return new WaitForSecondsRealtime(_winPopupDelaySeconds);

        if (_state == GameplayState.Completed)
        {
            _selectSongController?.Show();
        }

        _showSelectSongCoroutine = null;
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
