using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CatFeedBack : MonoBehaviour
{
    private const string SWEET = "Sweet!";
    private const string YUMMY = "Yummy!";
    private const string TASTY = "Tasty!";

    [SerializeField] private TextMeshPro _feedBackTMP;
    [SerializeField] private RectTransform _rectTf;
    [SerializeField] private float _hideDelaySeconds = 2f;
    [SerializeField] private float _punchMoveUpY = 0.5f;
    

    private static readonly string[] _feedbackTexts = { SWEET, YUMMY, TASTY };

    private Func<bool> _isEatingGetter;
    private Coroutine _feedbackTweenRoutine;
    private int _feedbackTweenVersion;
    private Vector2 _feedBackStartAnchoredPos;

    private void Awake()
    {
        if (_rectTf == null)
        {
            _rectTf = GetComponent<RectTransform>();
        }

        if (_feedBackTMP != null)
        {
            _feedBackStartAnchoredPos = _feedBackTMP.rectTransform.anchoredPosition;
            _feedBackTMP.gameObject.SetActive(false);
        }
    }

    public void Initialize(Func<bool> isEatingGetter)
    {
        _isEatingGetter = isEatingGetter;
    }

    public void UpdateTargetX(float targetX)
    {
        if (_rectTf == null)
        {
            return;
        }

        _rectTf.localScale = new Vector3(targetX, _rectTf.localScale.y, _rectTf.localScale.z);
    }

    public void OnCatEat()
    {
        if (_feedBackTMP == null)
        {
            return;
        }

        _feedBackTMP.gameObject.SetActive(true);
        _feedBackTMP.text = _feedbackTexts[UnityEngine.Random.Range(0, _feedbackTexts.Length)];
        FeedBackVisual(false);
    }

    private void FeedBackVisual(bool hideOnlyIfStillNotEating)
    {
        KillFeedbackTween();
        ResetFeedBackVisual();
        _feedbackTweenVersion++;
        int tweenVersion = _feedbackTweenVersion;
        _feedbackTweenRoutine = StartCoroutine(FeedBackVisualCoroutine(tweenVersion, hideOnlyIfStillNotEating));
    }

    public void HandleStateChanged()
    {
        KillFeedbackTween();

        if (_isEatingGetter != null && _isEatingGetter())
        {
            return;
        }

        FeedBackVisual(true);
    }

    private IEnumerator FeedBackVisualCoroutine(int tweenVersion, bool hideOnlyIfStillNotEating)
    {
        if (_feedBackTMP == null)
        {
            _feedbackTweenRoutine = null;
            yield break;
        }

        if (_hideDelaySeconds <= 0f)
        {
            ApplyPunchProgress(1f);
            if (!hideOnlyIfStillNotEating || (_isEatingGetter == null || !_isEatingGetter()))
            {
                HideFeedback();
            }

            _feedbackTweenRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < _hideDelaySeconds)
        {
            if (tweenVersion != _feedbackTweenVersion)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _hideDelaySeconds);
            ApplyPunchProgress(t);
            yield return null;
        }

        if (tweenVersion != _feedbackTweenVersion)
        {
            yield break;
        }

        ApplyPunchProgress(1f);

        if (!hideOnlyIfStillNotEating || (_isEatingGetter == null || !_isEatingGetter()))
        {
            HideFeedback();
        }

        _feedbackTweenRoutine = null;
    }

    private void HideFeedback()
    {
        if (_feedBackTMP == null)
        {
            return;
        }

        ResetFeedBackVisual();
        _feedBackTMP.gameObject.SetActive(false);
    }

    private void ApplyPunchProgress(float t)
    {
        if (_feedBackTMP == null)
        {
            return;
        }

        var anchoredPos = _feedBackStartAnchoredPos;
        anchoredPos.y += _punchMoveUpY * t;
        _feedBackTMP.rectTransform.anchoredPosition = anchoredPos;
        _feedBackTMP.alpha = Mathf.Lerp(1f, 0f, t);
    }

    private void ResetFeedBackVisual()
    {
        if (_feedBackTMP == null)
        {
            return;
        }

        _feedBackTMP.rectTransform.anchoredPosition = _feedBackStartAnchoredPos;
        _feedBackTMP.alpha = 1f;
    }

    private void KillFeedbackTween()
    {
        if (_feedbackTweenRoutine == null)
        {
            return;
        }

        StopCoroutine(_feedbackTweenRoutine);
        _feedbackTweenRoutine = null;
        _feedbackTweenVersion++;
    }
}


