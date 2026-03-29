using System;
using _Project.Scripts.Core.Gameplay.Tile;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TileSpriteConfig _tileSpriteConfig;

    public event Action<int> OnScoreChanged;

    public int CurrentScore { get; private set; }

    private void Start()
    {
        NotifyScoreChanged();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        NotifyScoreChanged();
    }

    public void AddScoreForTile(Tile tile)
    {
        if (tile == null)
        {
            return;
        }

        int scoreToAdd = ResolveScore(tile.TileSprite);
        AddScore(scoreToAdd);
    }

    public void AddScore(int score)
    {
        CurrentScore += Mathf.Max(0, score);
        NotifyScoreChanged();
    }

    private int ResolveScore(TileSprite tileSprite)
    {
        if (_tileSpriteConfig == null)
        {
            Debug.LogError("TileSpriteConfig is not assigned on ScoreManager.");
            return 0;
        }

        return _tileSpriteConfig.GetScore(tileSprite);
    }

    private void NotifyScoreChanged()
    {
        OnScoreChanged?.Invoke(CurrentScore);
    }
}

