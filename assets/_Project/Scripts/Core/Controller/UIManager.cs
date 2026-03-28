using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField, Min(1f)] private float scorePunchScale = 1.15f;
    [SerializeField, Min(0.01f)] private float scorePunchDuration = 0.15f;

    private Coroutine _scorePunchCoroutine;
    private Vector3 _scoreTextBaseScale = Vector3.one;
    private int _lastScore = int.MinValue;

    private void Awake()
    {
        if (scoreText != null)
        {
            _scoreTextBaseScale = scoreText.rectTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (scoreManager == null)
        {
            SetScoreText(0, false);
            return;
        }

        scoreManager.OnScoreChanged += OnScoreChanged;
        SetScoreText(scoreManager.CurrentScore, false);
    }

    private void OnDisable()
    {
        if (scoreManager == null)
        {
            ResetScoreTextScale();
            return;
        }

        scoreManager.OnScoreChanged -= OnScoreChanged;
        ResetScoreTextScale();
    }

    private void OnScoreChanged(int score)
    {
        SetScoreText(score, true);
    }

    private void SetScoreText(int score, bool allowPunch)
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text = score.ToString();
        bool shouldPunch = allowPunch && _lastScore != int.MinValue && score != _lastScore;
        _lastScore = score;

        if (shouldPunch)
        {
            PlayScorePunch();
        }
    }

    private void PlayScorePunch()
    {
        if (_scorePunchCoroutine != null)
        {
            StopCoroutine(_scorePunchCoroutine);
        }

        _scorePunchCoroutine = StartCoroutine(PunchScoreTextCoroutine());
    }

    private System.Collections.IEnumerator PunchScoreTextCoroutine()
    {
        RectTransform scoreTransform = scoreText.rectTransform;
        Vector3 baseScale = _scoreTextBaseScale;
        Vector3 punchScale = baseScale * scorePunchScale;
        float halfDuration = Mathf.Max(0.01f, scorePunchDuration * 0.5f);

        yield return LerpScale(scoreTransform, baseScale, punchScale, halfDuration);
        yield return LerpScale(scoreTransform, punchScale, baseScale, halfDuration);

        scoreTransform.localScale = baseScale;
        _scorePunchCoroutine = null;
    }

    private static System.Collections.IEnumerator LerpScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
    }

    private void ResetScoreTextScale()
    {
        if (_scorePunchCoroutine != null)
        {
            StopCoroutine(_scorePunchCoroutine);
            _scorePunchCoroutine = null;
        }

        if (scoreText != null)
        {
            scoreText.rectTransform.localScale = _scoreTextBaseScale;
        }
    }
}

