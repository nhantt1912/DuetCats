using System;
using UnityEngine;

namespace _Project.Scripts.Core.Adaptive
{
    [CreateAssetMenu(fileName = "AdaptiveViewProfile", menuName = "DuetCats/Adaptive View Profile")]
    public class AdaptiveViewProfile : ScriptableObject
    {
        [Header("Camera")]
        [SerializeField] private ModeCameraSettings _portraitCamera = new ModeCameraSettings
        {
            orthographicSize = 9.5f,
            localPosition = new Vector3(0f, 0f, -10f)
        };

        [SerializeField] private ModeCameraSettings _landscapeCamera = new ModeCameraSettings
        {
            orthographicSize = 7.5f,
            localPosition = new Vector3(0f, 0f, -10f)
        };

        [Header("Tile Layout")]
        [SerializeField] private ModeTileSettings _portraitTiles = new ModeTileSettings
        {
            laneLeftX = -2f,
            laneRightX = 2f,
            spawnY = 10f,
            hitY = -4.5f,
            despawnY = -8f
        };

        [SerializeField] private ModeTileSettings _landscapeTiles = new ModeTileSettings
        {
            laneLeftX = -3.5f,
            laneRightX = 3.5f,
            spawnY = 8f,
            hitY = -3.5f,
            despawnY = -7f
        };

        [Header("Cat Bounds")]
        [SerializeField] private ModeCatBounds _portraitLeftCat = new ModeCatBounds { limitLeft = -3.5f, limitRight = -1.5f };
        [SerializeField] private ModeCatBounds _portraitRightCat = new ModeCatBounds { limitLeft = 1.5f, limitRight = 3.5f };
        [SerializeField] private ModeCatBounds _landscapeLeftCat = new ModeCatBounds { limitLeft = -4.5f, limitRight = -1.8f };
        [SerializeField] private ModeCatBounds _landscapeRightCat = new ModeCatBounds { limitLeft = 1.8f, limitRight = 4.5f };

        public ModeCameraSettings GetCamera(AdaptiveViewMode mode)
        {
            return mode == AdaptiveViewMode.Landscape ? _landscapeCamera : _portraitCamera;
        }

        public ModeTileSettings GetTiles(AdaptiveViewMode mode)
        {
            return mode == AdaptiveViewMode.Landscape ? _landscapeTiles : _portraitTiles;
        }

        public ModeCatBounds GetLeftCatBounds(AdaptiveViewMode mode)
        {
            return mode == AdaptiveViewMode.Landscape ? _landscapeLeftCat : _portraitLeftCat;
        }

        public ModeCatBounds GetRightCatBounds(AdaptiveViewMode mode)
        {
            return mode == AdaptiveViewMode.Landscape ? _landscapeRightCat : _portraitRightCat;
        }

        [Serializable]
        public struct ModeCameraSettings
        {
            public float orthographicSize;
            public Vector3 localPosition;
        }

        [Serializable]
        public struct ModeTileSettings
        {
            public float laneLeftX;
            public float laneRightX;
            public float spawnY;
            public float hitY;
            public float despawnY;
        }

        [Serializable]
        public struct ModeCatBounds
        {
            public float limitLeft;
            public float limitRight;
        }
    }

    public enum AdaptiveViewMode
    {
        Portrait,
        Landscape
    }
}



