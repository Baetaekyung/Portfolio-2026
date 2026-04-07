using UnityEngine;

// RedDot UI 표시를 담당하는 컴포넌트
public class RedDotBehaviour : MonoBehaviour
{
    [SerializeField] private ERedDotKey redDotKey;
    [SerializeField] private GameObject redDotObject;

    private void OnEnable()
    {
        RedDotConditionManager.Instance.AddListener(redDotKey, OnRedDotChanged);
    }

    private void OnDisable()
    {
        RedDotConditionManager.Instance.RemoveListener(redDotKey, OnRedDotChanged);
    }

    // 상태 변경 콜백
    private void OnRedDotChanged(bool isActive)
    {
        if (redDotObject != null)
        {
            redDotObject.SetActive(isActive);
        }
    }
}
