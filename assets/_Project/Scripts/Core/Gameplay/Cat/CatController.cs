using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class CatController : MonoBehaviour
{
    [SerializeField] private float limitLeft;
    [SerializeField] private float limitRight;

    [SerializeField] private SkeletonAnimation _catSpine;
    
    private const string EATING_SINGLE = "Eating_Single_Object";
    private const string EATING_LONG_BEGIN = "Eat_Long_Begin";
    private const string EATING_LONG_LOOP = "Eat_Long_Loop";
    private const string EATING_LONG_END = "Eat_Long_End";
    private const string CAT_IDLE = "Idle_Playing";
    private const string CAT_LOSE = "Miss_Object_Lose";
    private const string CAT_WIN = "Cheering_Happy _Victory";
    
    private Coroutine _currentEatRoutine;

    public void MoveNormalized(float t, float deltaX)
    {
        float targetX = Mathf.Lerp(limitLeft, limitRight, t);

        if (Mathf.Abs(deltaX) > 0.01f)
        {
            float dir = Mathf.Sign(deltaX); 

            transform.localScale = new Vector3(
                dir * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
    
    public void PlayEatSingle()
    {
        Debug.Log("PlayEatSingle");
        
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        _catSpine.PlayAnimation(EATING_SINGLE,mixDuration:0.1f, loop:false, onComplete: () =>
        {
            _catSpine.PlayAnimation(CAT_IDLE,mixDuration:0.1f,loop: true);
        });
    }
    
    public void PlayEatLong(float duration)
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
        }
        _currentEatRoutine = StartCoroutine(EatLongRoutine(duration));
    }

    public void PlayLose()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        _catSpine.PlayAnimation(CAT_LOSE, mixDuration: 0.1f, loop: false);
    }

    public void PlayWin()
    {
        if (_currentEatRoutine != null)
        {
            StopCoroutine(_currentEatRoutine);
            _currentEatRoutine = null;
        }

        _catSpine.PlayAnimation(CAT_WIN, mixDuration: 0.1f, loop: false);
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
            _catSpine.PlayAnimation(CAT_IDLE,mixDuration:0.1f,loop: true);
        });
        
        _currentEatRoutine = null;
    }
}