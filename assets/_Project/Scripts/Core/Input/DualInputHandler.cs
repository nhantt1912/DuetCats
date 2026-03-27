using System.Collections.Generic;
using UnityEngine;

public class DualInputHandler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CatController leftCat;
    [SerializeField] private CatController rightCat;

    [Header("Config")]
    [SerializeField] private float deadZone = 30f;

    private Dictionary<int, InputSide> fingerSides = new();
    
    private Dictionary<int, float> lastPosX = new();

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
            fingerSides.Remove(0);
    }

    private void AssignFinger(int id, float x)
    {
        if (x < _midX - deadZone)
            fingerSides[id] = InputSide.Left;
        else if (x > _midX + deadZone)
            fingerSides[id] = InputSide.Right;

        lastPosX[id] = x;
    }

    private void UpdateFinger(int id, float x)
    {
        if (!fingerSides.TryGetValue(id, out var side)) return;

        float deltaX = x - lastPosX[id];
        lastPosX[id] = x;

        if (side == InputSide.Left)
        {
            float t = Mathf.Clamp01(x / _halfWidth);
            leftCat.MoveNormalized(t, deltaX);
        }
        else
        {
            float t = Mathf.Clamp01((x - _midX) / _halfWidth);
            rightCat.MoveNormalized(t, deltaX);
        }
    }
}
public enum InputSide
{
    None,
    Left,
    Right
}