using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Gameplay.Tile;
using UnityEngine;
using UnityEngine.Pool;

public class TileManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _fallSpeed = 5f;
    [SerializeField] private float _spawnY = 10;
    [SerializeField] private float _despawnY = -10f;

    [Header("Rhythm Settings")]
    [SerializeField] private float _hitY = -3f;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _preStartDelaySeconds = 1.5f;
    [SerializeField] private bool _requireTapToStart = true;

    [Header("Lane Settings (X Positions)")]
    [SerializeField] private float _laneLeftX = -2f;
    [SerializeField] private float _laneRightX = 2f;

    [Header("References")]
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _tileParent;
    [SerializeField] private TextAsset _jsonData;

    [Header("Sprite Configuration")]
    [SerializeField] private TileSpriteConfig _tileSpriteConfig;

    [Header("Tile Logic Thresholds")]
    [SerializeField] private float _rapidTsThreshold = 0.25f;

    [SerializeField] private int _strongVolumeThreshold = 100;
    
    private List<Tile> _activeTiles = new List<Tile>();

    public Action<Tile> OnTileHitZone;
    public Action OnLose;
    
    private List<TileData> _datas;
    private ObjectPool<Tile> _pool;
    
    private float _travelTime;
    private int _nextSpawnIndex = 0;
    private bool _isPlaying = false;
    private bool _isWaitingForStartTap = false;
    private double _scheduledDspStartTime;

    
    private void Awake()
    {
        TileDataList wrapper = JsonUtility.FromJson<TileDataList>(_jsonData.text);
        _datas = wrapper.items;

        _datas.Sort((a, b) => a.ta.CompareTo(b.ta));

        _pool = new ObjectPool<Tile>(
            createFunc: () => Instantiate(_tilePrefab, _tileParent),
            actionOnGet: (t) => {
                t.gameObject.SetActive(true);
                _activeTiles.Add(t);
            },
            actionOnRelease: (t) => {
                t.gameObject.SetActive(false);
                _activeTiles.Remove(t);
            },
            defaultCapacity: 50
        );
    }

    private void Start()
    {   
        _travelTime = (_spawnY - _hitY) / _fallSpeed;

        if (_audioSource == null || _audioSource.clip == null)
        {
            _isPlaying = false;
            _isWaitingForStartTap = false;
            return;
        }

        if (_requireTapToStart)
        {
            _isPlaying = false;
            _isWaitingForStartTap = true;
        }
        else
        {
            BeginGameplay();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;
        
        float songTime = GetSongTime();

        while (_nextSpawnIndex < _datas.Count && songTime >= _datas[_nextSpawnIndex].ta - _travelTime)
        {
            Spawn(_datas[_nextSpawnIndex]);
            _nextSpawnIndex++;
        }

        MoveTiles(songTime);
    }

    public bool IsWaitingForStartTap => _isWaitingForStartTap;
    public int TotalTileCount => _datas?.Count ?? 0;

    public void ResetGameplayState(bool waitForStartTap)
    {
        _isPlaying = false;
        _isWaitingForStartTap = waitForStartTap;
        _nextSpawnIndex = 0;
        _scheduledDspStartTime = 0d;

        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.time = 0f;
        }

        ReleaseAllActiveTiles();
    }

    public void ReleaseAllActiveTiles()
    {
        if (_pool == null)
        {
            return;
        }

        for (int i = _activeTiles.Count - 1; i >= 0; i--)
        {
            Tile tile = _activeTiles[i];
            if (tile != null)
            {
                _pool.Release(tile);
            }
        }
    }

    public void BeginGameplay()
    {
        if (_isPlaying)
        {
            return;
        }
        float delay = Mathf.Max(Mathf.Max(0f, _preStartDelaySeconds), _travelTime);
        _scheduledDspStartTime = AudioSettings.dspTime + delay;
        _audioSource.time = 0f;
        _audioSource.PlayScheduled(_scheduledDspStartTime);

        _isWaitingForStartTap = false;
        _isPlaying = true;
    }

    private float GetSongTime()
    {
        float elapsed = (float)(AudioSettings.dspTime - _scheduledDspStartTime);
        return Mathf.Min(elapsed, _audioSource.clip.length + _travelTime);
    }
    
    private void MoveTiles(float songTime)
    {
        for (int i = _activeTiles.Count - 1; i >= 0; i--)
        {
            Tile tile = _activeTiles[i];

            float timeUntilHit = tile.Data.ta - songTime;
            float newY = _hitY + (timeUntilHit * _fallSpeed);
            newY = Mathf.Min(newY, _spawnY);

            tile.transform.position = new Vector3(tile.transform.position.x, newY, 0);

            if (tile.transform.position.y < _despawnY)
            {
                OnLoseGame(tile);
                return;
            }

            if (!tile.IsProcessed && songTime >= tile.Data.ta)
            {
                tile.SetProcessed(true);
                OnTileHitZone?.Invoke(tile);
            }
            
            
        }
    }

    private void OnLoseGame(Tile failedTile)
    {
        if (!_isPlaying) return;

        _isPlaying = false;
        OnLose?.Invoke();

        if (_audioSource != null)
        {
            _audioSource.Stop();
        }

        if (failedTile == null || failedTile.Data == null)
        {
            return;
        }

        bool isLeft = IsLeftLane(failedTile.Data);
        Sprite breakSprite = ResolveSprite(isLeft, TileSprite.Break);

        if (breakSprite != null)
        {
            failedTile.SetSprite(breakSprite);
        }
    }


    private void Spawn(TileData data)
    {
        Tile t = _pool.Get();
        bool isLeft = IsLeftLane(data);
        TileSprite tileSprite = GetTileSprite(data);

        Sprite selectedSprite = ResolveSprite(isLeft, tileSprite);

        float startX = isLeft ? _laneLeftX : _laneRightX;
        t.transform.position = new Vector3(startX, _spawnY, 0f);

        t.Init(data, selectedSprite, tileSprite);
    }

    private bool IsLeftLane(TileData data)
    {
        return data.pid == 0 || data.pid == 2;
    }

    private TileSprite GetTileSprite(TileData data)
    {
        if (data.v >= _strongVolumeThreshold)
        {
            return TileSprite.Strong;
        }

        if (_rapidTsThreshold > 0f && data.ts > 0f && data.ts <= _rapidTsThreshold)
        {
            return TileSprite.Rapid;
        }

        return TileSprite.Normal;
    }

    private Sprite ResolveSprite(bool isLeft, TileSprite tileSprite)
    {
        if (_tileSpriteConfig == null)
        {
            Debug.Log("TileSpriteConfig is not assigned on TileManager.");
            return null;
        }

        Sprite spriteFromConfig = _tileSpriteConfig.GetSprite(isLeft, tileSprite);
        if (spriteFromConfig == null)
        {
            Debug.Log($"Missing sprite in TileSpriteConfig for lane={(isLeft ? "Left" : "Right")}, type={tileSprite}.");
        }

        return spriteFromConfig;
    }

    public void Release(Tile tile)
    {
        _pool.Release(tile);
    }
}

[System.Serializable]
public class TileData
{
    public int id;
    public int n;
    public float ta;
    public float ts;
    public float d;
    public int v;
    public int pid;
}

[System.Serializable]
public class TileDataList
{
    public List<TileData> items;
}
