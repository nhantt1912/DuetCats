using System.Collections.Generic;
using UnityEngine;

public class DualInputHandler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CatController _leftCat;
    [SerializeField] private CatController _rightCat;

    [Header("Config")]
    [SerializeField] private float _deadZone = 30f;

    private Dictionary<int, InputSide> _fingerSides = new();
    
    private Dictionary<int, float> _lastPosX = new();

    private float _midX;
    private float _halfWidth;

    private void Awake()
    {
        _midX = Screen.width * 0.5f;
        _halfWidth = Screen.width * 0.5f;
    }

    private void Update()
    {
        HandleMouse();
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            AssignFinger(0, Input.mousePosition.x);

        if (Input.GetMouseButton(0))
            UpdateFinger(0, Input.mousePosition.x);

        if (Input.GetMouseButtonUp(0))
            _fingerSides.Remove(0);
    }

    private void AssignFinger(int id, float x)
    {
        if (x < _midX - _deadZone)
            _fingerSides[id] = InputSide.Left;
        else if (x > _midX + _deadZone)
            _fingerSides[id] = InputSide.Right;

        _lastPosX[id] = x;
    }

    private void UpdateFinger(int id, float x)
    {
        if (!_fingerSides.TryGetValue(id, out var side)) return;

        float deltaX = x - _lastPosX[id];
        _lastPosX[id] = x;

        if (side == InputSide.Left)
        {
            float t = Mathf.Clamp01(x / _halfWidth);
            _leftCat.MoveNormalized(t, deltaX);
        }
        else
        {
            float t = Mathf.Clamp01((x - _midX) / _halfWidth);
            _rightCat.MoveNormalized(t, deltaX);
        }
    }
}
public enum InputSide
{
    None,
    Left,
    Right
}