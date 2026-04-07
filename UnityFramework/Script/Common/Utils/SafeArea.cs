using UnityEngine;

/// <summary>
/// 모바일 디바이스의 SafeArea를 적용하는 컴포넌트
/// 노치, 홈 인디케이터 등을 피해 UI를 안전하게 배치
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private bool applyTop = true;
    [SerializeField] private bool applyBottom = true;
    [SerializeField] private bool applyLeft = true;
    [SerializeField] private bool applyRight = true;

    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private ScreenOrientation _lastOrientation;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        // 화면 크기나 방향이 변경되면 SafeArea 재적용
        if (HasScreenChanged())
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// 화면 상태 변경 여부 확인
    /// </summary>
    private bool HasScreenChanged()
    {
        var currentSafeArea = Screen.safeArea;
        var currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        var currentOrientation = Screen.orientation;

        if (_lastSafeArea != currentSafeArea ||
            _lastScreenSize != currentScreenSize ||
            _lastOrientation != currentOrientation)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// SafeArea 적용
    /// </summary>
    public void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;

        // 현재 상태 저장
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastOrientation = Screen.orientation;

        // 화면 크기
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        if (screenWidth <= 0 || screenHeight <= 0)
        {
            return;
        }

        // SafeArea를 0~1 범위로 정규화
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= screenWidth;
        anchorMin.y /= screenHeight;
        anchorMax.x /= screenWidth;
        anchorMax.y /= screenHeight;

        // 적용할 방향에 따라 anchor 조정
        if (!applyLeft)
        {
            anchorMin.x = 0f;
        }
        if (!applyRight)
        {
            anchorMax.x = 1f;
        }
        if (!applyBottom)
        {
            anchorMin.y = 0f;
        }
        if (!applyTop)
        {
            anchorMax.y = 1f;
        }

        // RectTransform에 적용
        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;

#if UNITY_EDITOR
        Debug.Log($"[SafeArea] 적용됨 - SafeArea: {safeArea}, AnchorMin: {anchorMin}, AnchorMax: {anchorMax}");
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 테스트용 SafeArea 시뮬레이션
    /// </summary>
    [ContextMenu("Apply SafeArea (Editor)")]
    private void ApplySafeAreaEditor()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    /// <summary>
    /// SafeArea 초기화
    /// </summary>
    [ContextMenu("Reset SafeArea")]
    private void ResetSafeArea()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.one;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
#endif
}
