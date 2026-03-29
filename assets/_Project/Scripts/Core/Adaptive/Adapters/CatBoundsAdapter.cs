using System.Reflection;
using UnityEngine;
using _Project.Scripts.Core.Adaptive;

namespace _Project.Scripts.Core.Adaptive.Adapters
{
    [DisallowMultipleComponent]
    public class CatBoundsAdapter : MonoBehaviour
    {
        [SerializeField] private CatController _catController;

        private static readonly BindingFlags _fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static FieldInfo _limitLeftField;
        private static FieldInfo _limitRightField;

        public void Configure(CatController controller)
        {
            _catController = controller;
        }

        public void Apply(AdaptiveViewProfile.ModeCatBounds settings)
        {
            if (_catController == null)
            {
                return;
            }

            CacheFields();
            _limitLeftField?.SetValue(_catController, settings.limitLeft);
            _limitRightField?.SetValue(_catController, settings.limitRight);
        }

        private void CacheFields()
        {
            if (_limitLeftField != null)
            {
                return;
            }

            System.Type catType = typeof(CatController);
            _limitLeftField = catType.GetField("_limitLeft", _fieldFlags);
            _limitRightField = catType.GetField("_limitRight", _fieldFlags);
        }
    }
}



