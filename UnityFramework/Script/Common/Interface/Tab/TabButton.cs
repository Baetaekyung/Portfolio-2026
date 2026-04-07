using UnityEngine;
using UnityEngine.UI;

public class TabButton : Button
{
    [SerializeField] private Tab tab;

    private TabController _controller;

    public Tab Tab => tab;

    public void Initialize(TabController controller)
    {
        _controller = controller;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.AddListener(OnClickButton);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onClick.RemoveListener(OnClickButton);
    }

    // 메서드 참조로 등록하면 구독 취소 가능
    private void OnClickButton()
    {
        if (tab != null)
        {
            tab.SelectTab();
        }
    }
}
