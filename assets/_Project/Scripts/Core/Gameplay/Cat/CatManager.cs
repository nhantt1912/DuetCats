using System;
using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private CatController _catLeftController;
    [SerializeField] private CatController _catRightController;
    [SerializeField] private float _longEatDurationThreshold = 0.2f;

    public event Action<bool> OnAnyCatEatingStateChanged;

    public bool AreAnyCatsEating =>
        (_catLeftController != null && _catLeftController.IsEating) ||
        (_catRightController != null && _catRightController.IsEating);

    private void OnEnable()
    {
        SubscribeEatingStateEvents();
        NotifyAnyCatEatingStateChanged();
    }

    private void OnDisable()
    {
        UnsubscribeEatingStateEvents();
    }

    private void SubscribeEatingStateEvents()
    {
        if (_catLeftController != null)
        {
            _catLeftController.OnStateChanged += HandleCatStateChanged;
        }

        if (_catRightController != null)
        {
            _catRightController.OnStateChanged += HandleCatStateChanged;
        }
    }

    private void UnsubscribeEatingStateEvents()
    {
        if (_catLeftController != null)
        {
            _catLeftController.OnStateChanged -= HandleCatStateChanged;
        }

        if (_catRightController != null)
        {
            _catRightController.OnStateChanged -= HandleCatStateChanged;
        }
    }

    private void HandleCatStateChanged(CatController.CatState _)
    {
        NotifyAnyCatEatingStateChanged();
    }

    private void NotifyAnyCatEatingStateChanged()
    {
        OnAnyCatEatingStateChanged?.Invoke(AreAnyCatsEating);
    }

    public void PlayLoseAnimation()
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayAnimationLose();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayAnimationLose();
        }
    }

    public void PlayAnimationCatWin()
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayAnimationWin();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayAnimationWin();
        }
    }

    public void ResetCatsToIdle()
    {
        if (_catLeftController != null)
        {
            _catLeftController.ResetToIdle();
        }

        if (_catRightController != null)
        {
            _catRightController.ResetToIdle();
        }

        NotifyAnyCatEatingStateChanged();
    }

    public bool TryEatTile(Tile tile, float hitToleranceX,Action OnEatComplete = null)
    {
        int pid = tile.Data.pid;
        CatController targetCat = (pid == 0 || pid == 2) ? _catLeftController : _catRightController;

        float catX = targetCat.transform.position.x;
        float tileX = tile.transform.position.x;

        if (Mathf.Abs(catX - tileX) <= hitToleranceX)
        {
            float noteDuration = tile.Data.d;

            if (noteDuration > _longEatDurationThreshold)
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
    
    public void PlayListeningAnimation()
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayAnimationListening();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayAnimationListening();
        }
    }
}
