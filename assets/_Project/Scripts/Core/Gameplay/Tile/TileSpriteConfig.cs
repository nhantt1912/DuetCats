using System;
using UnityEngine;

namespace _Project.Scripts.Core.Gameplay.Tile
{
    [CreateAssetMenu(fileName = "TileSpriteConfig", menuName = "DuetCats/Tile Sprite Config")]
    public class TileSpriteConfig : ScriptableObject
    {
        [Serializable]
        public struct LaneSprites
        {
            public Sprite normal;
            public Sprite rapid;
            public Sprite strong;
            public Sprite breakSprite;

            public Sprite Get(TileSprite tileSprite)
            {
                switch (tileSprite)
                {
                    case TileSprite.Rapid:
                        return rapid;
                    case TileSprite.Strong:
                        return strong;
                    case TileSprite.Break:
                        return breakSprite;
                    default:
                        return normal;
                }
            }
        }

        [Header("Lane Sprite Sets")]
        [SerializeField] private LaneSprites _left;
        [SerializeField] private LaneSprites _right;

        [Header("Score Rules")]
        [SerializeField] private int _normalScore = 2;
        [SerializeField] private int _rapidScore = 2;
        [SerializeField] private int _strongScore = 10;
        [SerializeField] private int _breakScore = 0;

        public Sprite GetSprite(bool isLeft, TileSprite tileSprite)
        {
            return isLeft ? _left.Get(tileSprite) : _right.Get(tileSprite);
        }

        public int GetScore(TileSprite tileSprite)
        {
            switch (tileSprite)
            {
                case TileSprite.Strong:
                    return _strongScore;
                case TileSprite.Rapid:
                    return _rapidScore;
                case TileSprite.Break:
                    return _breakScore;
                default:
                    return _normalScore;
            }
        }
    }
}
