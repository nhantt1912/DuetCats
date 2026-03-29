using System;
using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private CatController _catLeftController;
    [SerializeField] private CatController _catRightController;
    [SerializeField] private float longEatDurationThreshold = 0.2f;

    public event Action OnWinAnimationFinished;

    public void PlayLoseAnimation(Tile failedTile)
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayLose();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayLose();
        }
    }

    public void PlayWinAnimation()
    {
        int pendingWinCompleteCount = 0;

        void HandleWinComplete()
        {
            pendingWinCompleteCount--;
            if (pendingWinCompleteCount <= 0)
            {
                OnWinAnimationFinished?.Invoke();
            }
        }

        if (_catLeftController != null)
        {
            pendingWinCompleteCount++;
            _catLeftController.PlayWin(HandleWinComplete);
        }

        if (_catRightController != null)
        {
            pendingWinCompleteCount++;
            _catRightController.PlayWin(HandleWinComplete);
        }

        if (pendingWinCompleteCount == 0)
        {
            OnWinAnimationFinished?.Invoke();
        }
    }


    public bool TryEatTile(Tile tile, float hitToleranceX)
    {
        int pid = tile.Data.pid;
        CatController targetCat = (pid == 0 || pid == 2) ? _catLeftController : _catRightController;

        float catX = targetCat.transform.position.x;
        float tileX = tile.transform.position.x;

        if (Mathf.Abs(catX - tileX) <= hitToleranceX)
        {
            float noteDuration = tile.Data.d;

            if (noteDuration > longEatDurationThreshold)
            {
                targetCat.PlayEatLong(noteDuration);
            }
            else
            {
                targetCat.PlayEatSingle();
            }

            return true; 
        }

        return false; 
    }
    
}