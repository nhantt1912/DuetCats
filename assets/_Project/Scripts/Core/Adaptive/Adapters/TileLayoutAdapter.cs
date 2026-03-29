using System.Reflection;
using UnityEngine;
using _Project.Scripts.Core.Adaptive;

namespace _Project.Scripts.Core.Adaptive.Adapters
{
    [DisallowMultipleComponent]
    public class TileLayoutAdapter : MonoBehaviour
    {
        [SerializeField] private TileManager _tileManager;

        private static readonly BindingFlags _fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static FieldInfo _laneLeftXField;
        private static FieldInfo _laneRightXField;
        private static FieldInfo _spawnYField;
        private static FieldInfo _hitYField;
        private static FieldInfo _despawnYField;

        public void Configure(TileManager manager)
        {
            _tileManager = manager;
        }

        public void Apply(AdaptiveViewProfile.ModeTileSettings settings)
        {
            if (_tileManager == null)
            {
                return;
            }

            CacheFields();
            SetField(_laneLeftXField, settings.laneLeftX);
            SetField(_laneRightXField, settings.laneRightX);
            SetField(_spawnYField, settings.spawnY);
            SetField(_hitYField, settings.hitY);
            SetField(_despawnYField, settings.despawnY);
        }

        private void SetField(FieldInfo field, float value)
        {
            field?.SetValue(_tileManager, value);
        }

        private void CacheFields()
        {
            if (_laneLeftXField != null)
            {
                return;
            }

            System.Type tileManagerType = typeof(TileManager);
            _laneLeftXField = tileManagerType.GetField("_laneLeftX", _fieldFlags);
            _laneRightXField = tileManagerType.GetField("_laneRightX", _fieldFlags);
            _spawnYField = tileManagerType.GetField("_spawnY", _fieldFlags);
            _hitYField = tileManagerType.GetField("_hitY", _fieldFlags);
            _despawnYField = tileManagerType.GetField("_despawnY", _fieldFlags);
        }
    }
}



