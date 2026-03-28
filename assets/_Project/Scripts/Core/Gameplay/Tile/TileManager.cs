using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Gameplay.Tile;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class TileManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float fallSpeed = 5f; 
    [SerializeField] private float spawnY = 10;   
    [SerializeField] private float despawnY = -10f; 

    [Header("Rhythm Settings")]
    [SerializeField] private float hitY = -3f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float preStartDelaySeconds = 1.5f;
    [SerializeField] private float resetDelaySeconds = 5f;
    [SerializeField] private bool requireTapToStart = true;

    [Header("Lane Settings (X Positions)")]
    [SerializeField] private float laneLeftX = -2f;  
    [SerializeField] private float laneRightX = 2f; 

    [Header("References")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform tileParent;
    [SerializeField] private TextAsset _jsonData;

    [Header("Sprite Configuration")]
    [SerializeField] private TileSpriteConfig tileSpriteConfig;

    [Header("Tile Logic Thresholds")]
    [SerializeField] private float rapidTsThreshold = 0.25f;

    [SerializeField] private int strongVolumeThreshold = 100;
    
    private List<Tile> activeTiles = new List<Tile>();

    public Action<Tile> OnTileHitZone;
    public Action<Tile> OnLose;
    
    private List<TileData> datas;
    private ObjectPool<Tile> pool;
    
    private float travelTime;
    private int nextSpawnIndex = 0;
    private bool isPlaying = false;
    private bool isWaitingForStartTap = false;
    private double scheduledDspStartTime;
    private bool isResetScheduled = false;

    
    private void Awake()
    {
        TileDataList wrapper = JsonUtility.FromJson<TileDataList>(_jsonData.text);
        datas = wrapper.items;
        
        datas.Sort((a, b) => a.ta.CompareTo(b.ta)); 

        pool = new ObjectPool<Tile>(
            createFunc: () => Instantiate(tilePrefab, tileParent),
            actionOnGet: (t) => {
                t.gameObject.SetActive(true);
                activeTiles.Add(t);
            },
            actionOnRelease: (t) => {
                t.gameObject.SetActive(false);
                activeTiles.Remove(t);
            },
            defaultCapacity: 50
        );
    }

    private void Start()
    {   
        travelTime = (spawnY - hitY) / fallSpeed;

        if (audioSource == null || audioSource.clip == null)
        {
            isPlaying = false;
            isWaitingForStartTap = false;
            return;
        }

        if (requireTapToStart)
        {
            isPlaying = false;
            isWaitingForStartTap = true;
        }
        else
        {
            BeginGameplay();
        }
    }

    private void Update()
    {
        if (!isPlaying) return;
        
        float songTime = GetSongTime();

        while (nextSpawnIndex < datas.Count && songTime >= datas[nextSpawnIndex].ta - travelTime)
        {
            Spawn(datas[nextSpawnIndex]);
            nextSpawnIndex++;
        }

        MoveTiles(songTime);
    }

    public bool IsWaitingForStartTap => isWaitingForStartTap;
    public int TotalTileCount => datas?.Count ?? 0;

    public void BeginGameplay()
    {
        if (isPlaying)
        {
            return;
        }

        float delay = Mathf.Max(Mathf.Max(0f, preStartDelaySeconds), travelTime);
        scheduledDspStartTime = AudioSettings.dspTime + delay;
        audioSource.time = 0f;
        audioSource.PlayScheduled(scheduledDspStartTime);

        isWaitingForStartTap = false;
        isPlaying = true;
    }

    private float GetSongTime()
    {
        float elapsed = (float)(AudioSettings.dspTime - scheduledDspStartTime);
        return Mathf.Min(elapsed, audioSource.clip.length + travelTime);
    }
    
    private void MoveTiles(float songTime)
    {
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            Tile tile = activeTiles[i];

            float timeUntilHit = tile.Data.ta - songTime;
            float newY = hitY + (timeUntilHit * fallSpeed);
            newY = Mathf.Min(newY, spawnY);

            tile.transform.position = new Vector3(tile.transform.position.x, newY, 0);

            if (tile.transform.position.y < despawnY)
            {
                OnLoseGame(tile);
                return;
            }

            if (!tile.IsProcessed && songTime >= tile.Data.ta)
            {
                tile.SetProcessed(true);
                OnTileHitZone?.Invoke(tile);
                Debug.Log("Cat eats tile");
            }
            
            
        }
    }

    private void OnLoseGame(Tile failedTile)
    {
        if (!isPlaying) return;

        isPlaying = false;
        OnLose?.Invoke(failedTile);

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (!isResetScheduled)
        {
            isResetScheduled = true;
            StartCoroutine(ResetGameplayAfterDelay());
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

    private System.Collections.IEnumerator ResetGameplayAfterDelay()
    {
        yield return new WaitForSecondsRealtime(resetDelaySeconds);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void Spawn(TileData data)
    {
        Tile t = pool.Get();
        bool isLeft = IsLeftLane(data);
        TileSprite tileSprite = GetTileSprite(data);

        Sprite selectedSprite = ResolveSprite(isLeft, tileSprite);

        float startX = isLeft ? laneLeftX : laneRightX;
        t.transform.position = new Vector3(startX, spawnY, 0f);

        t.Init(data, selectedSprite, tileSprite);
    }

    private bool IsLeftLane(TileData data)
    {
        return data.pid == 0 || data.pid == 2;
    }

    private TileSprite GetTileSprite(TileData data)
    {
        if (data.v >= strongVolumeThreshold)
        {
            return TileSprite.Strong;
        }

        if (data.ts <= rapidTsThreshold)
        {
            return TileSprite.Rapid;
        }

        return TileSprite.Normal;
    }

    private Sprite ResolveSprite(bool isLeft, TileSprite tileSprite)
    {
        if (tileSpriteConfig == null)
        {
            Debug.Log("TileSpriteConfig is not assigned on TileManager.");
            return null;
        }

        Sprite spriteFromConfig = tileSpriteConfig.GetSprite(isLeft, tileSprite);
        if (spriteFromConfig == null)
        {
            Debug.Log($"Missing sprite in TileSpriteConfig for lane={(isLeft ? "Left" : "Right")}, type={tileSprite}.");
        }

        return spriteFromConfig;
    }

    public void Release(Tile tile)
    {
        pool.Release(tile);
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
