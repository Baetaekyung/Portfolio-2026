using System;
using UnityEngine;
using UnityEngine.InputSystem;

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class InputManager : Singleton<InputManager>
{
    // 클릭/터치 이벤트 (위치 포함)
    public event Action<Vector2> OnClick;
    public event Action<Vector2> OnRelease;
    public event Action<Vector2> OnDrag;

    private bool _isPressed;
    private Vector2 _lastPosition;

    // 현재 프레임에 클릭되었는지
    public bool WasPressedThisFrame { get; private set; }
    // 현재 프레임에 릴리즈되었는지
    public bool WasReleasedThisFrame { get; private set; }
    // 현재 누르고 있는지
    public bool IsPressed => _isPressed;
    // 마지막 입력 위치
    public Vector2 Position => _lastPosition;

    private void Update()
    {
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;

        var pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        var pressed = pointer.press.isPressed;
        var position = pointer.position.ReadValue();

        // 클릭 시작 감지
        if (pressed && _isPressed == false)
        {
            WasPressedThisFrame = true;
            _lastPosition = position;
            OnClick?.Invoke(position);
        }
        // 클릭 종료 감지
        else if (pressed == false && _isPressed)
        {
            WasReleasedThisFrame = true;
            _lastPosition = position;
            OnRelease?.Invoke(position);
        }
        // 드래그 중
        else if (pressed && _isPressed)
        {
            _lastPosition = position;
            OnDrag?.Invoke(position);
        }

        _isPressed = pressed;
    }
}
