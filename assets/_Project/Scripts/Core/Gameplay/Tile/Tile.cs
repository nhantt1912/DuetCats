using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using _Project.Scripts.Core.Gameplay.Tile;

public class Tile : MonoBehaviour
    {
        [SerializeField] private TileType _type;
        public TileType Type => _type;
        [FormerlySerializedAs("_tile")] [SerializeField] private SpriteRenderer _srTile;
        
        private TileData _data;
        public TileData Data => _data;

        private TileSprite _tileSprite;
        public TileSprite TileSprite => _tileSprite;
        
        private bool _isProcessed = false;
        public bool IsProcessed => _isProcessed;
        
        public void Init(TileData data, Sprite spriteToUse, TileSprite tileSprite)
        {
          _data = data;
          _tileSprite = tileSprite;
          _srTile.sprite = spriteToUse;
          _isProcessed = false;
        }

        public void SetSprite(Sprite spriteToUse)
        {
            _srTile.sprite = spriteToUse;
        }

        public void SetProcessed(bool value)
        {
            _isProcessed = value;
        }
    }
