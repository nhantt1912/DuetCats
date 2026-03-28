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
        [SerializeField] private LaneSprites left;
        [SerializeField] private LaneSprites right;

        [Header("Score Rules")]
        [SerializeField] private int normalScore = 2;
        [SerializeField] private int rapidScore = 2;
        [SerializeField] private int strongScore = 10;
        [SerializeField] private int breakScore = 0;

        public Sprite GetSprite(bool isLeft, TileSprite tileSprite)
        {
            return isLeft ? left.Get(tileSprite) : right.Get(tileSprite);
        }

        public int GetScore(TileSprite tileSprite)
        {
            switch (tileSprite)
            {
                case TileSprite.Strong:
                    return strongScore;
                case TileSprite.Rapid:
                    return rapidScore;
                case TileSprite.Break:
                    return breakScore;
                default:
                    return normalScore;
            }
        }
    }
}
