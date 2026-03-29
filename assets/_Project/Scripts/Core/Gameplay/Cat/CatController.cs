using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public enum CatState
    {
        Idle,
        EatingSingle,
        EatingLong,
        Listening,
        Lose,
        Win
    }

    [SerializeField] private float _limitLeft;
    [SerializeField] private float _limitRight;

    [SerializeField] private SkeletonAnimation _catSpine;
    [SerializeField] private CatFeedBack _catFeedBack;

    private const string EATING_SINGLE = "Eating_Single_Object";
    private const string EATING_LONG_BEGIN = "Eat_Long_Begin";
    private const string EATING_LONG_LOOP = "Eat_Long_Loop";
    private const string EATING_LONG_END = "Eat_Long_End";
    private const string CAT_IDLE = "Idle_Playing";
    private const string CAT_LOSE_1 = "Miss_Object_Lose";
    private const string CAT_LOSE_2 = "Miss_Object_Lose_2";
    private const string WIN_ANIMATION_FALLBACK = "Cheering_Happy _Victory";
    private const string LISTENING = "Listening";
    

    private Coroutine _currentEatRoutine;

    public CatState State { get; private set; } = CatState.Idle;
    public bool IsEating => State == CatState.EatingSingle || State == CatState.EatingLong;
    public event Action<CatState> OnStateChanged;

    private void Awake()
    {
        _catFeedBack?.Initialize(() => IsEating);
        SyncFeedBackScaleX();
    }

    private void SyncFeedBackScaleX()
    {
        _catFeedBack?.UpdateTargetX(transform.localScale.x);
    }

    private void SetState(CatState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        OnStateChanged?.Invoke(State);
        _catFeedBack?.HandleStateChanged();
    }

    public void MoveNormalized(float t, float deltaX)
    {
        float targetX = Mathf.Lerp(_limitLeft, _limitRight, t);

        if (Mathf.Abs(deltaX) > 0.01f)
        {
            float dir = Mathf.Sign(deltaX);

            transform.localScale = new Vector3(
                dir * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        SyncFeedBackScaleX();

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
    
    public void PlayEatSingle()
    {
        SetState(CatState.EatingSingle);
        _catFeedBack?.OnCatEat();
        
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        _catSpine.PlayAnimation(EATING_SINGLE,mixDuration:0.1f, loop:false, onComplete: () =>
        {
            SetState(CatState.Idle);
            _catSpine.PlayAnimation(CAT_IDLE,mixDuration:0.1f,loop: true);
        });
    }
    
    public void PlayEatLong(float duration)
    {
        SetState(CatState.EatingLong);
        _catFeedBack?.OnCatEat();

        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        _currentEatRoutine = StartCoroutine(EatLongRoutine(duration));
    }


    public void PlayAnimationLose()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        SetState(CatState.Lose);
        _catSpine.PlayAnimation(CAT_LOSE_1, mixDuration: 0.1f, loop: false);
    }

    public void PlayAnimationWin()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        SetState(CatState.Win);
        _catSpine.PlayAnimation(WIN_ANIMATION_FALLBACK, mixDuration: 0.1f, loop: false);
        Debug.Log("PlayAnimationWin");
    }

    public void PlayAnimationListening()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        SetState(CatState.Listening);
        _catSpine.PlayAnimation(LISTENING, mixDuration: 0.1f, loop: false, onComplete: () =>
        {
            SetState(CatState.Idle);
            _catSpine.PlayAnimation(CAT_IDLE, mixDuration: 0.1f, loop: true);
        });
    }

    public void ResetToIdle()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        SetState(CatState.Idle);
        if (_catSpine != null)
        {
            _catSpine.PlayAnimation(CAT_IDLE, mixDuration: 0.1f, loop: true);
        }
    }

    private IEnumerator EatLongRoutine(float duration)
    {
        Debug.Log("PlayEatLong");   
        _catSpine.PlayAnimation(EATING_LONG_BEGIN,mixDuration:0.1f, loop:false, onComplete: () => 
        {
            _catSpine.PlayAnimation(EATING_LONG_LOOP,mixDuration:0.1f,loop: true);
        });

        yield return new WaitForSeconds(duration);

        _catSpine.PlayAnimation(EATING_LONG_END,mixDuration:0.1f, loop:false, onComplete: () => 
        {

            SetState(CatState.Idle);
            _catSpine.PlayAnimation(CAT_IDLE,mixDuration:0.1f,loop: true);
        });

        _currentEatRoutine = null;
    }
}