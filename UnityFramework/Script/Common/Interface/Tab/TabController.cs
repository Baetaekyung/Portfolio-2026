using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    [SerializeField] private List<TabButton> tabButtons;

    private void Awake()
    {
        // 각 TabButton 초기화
        foreach (var tabButton in tabButtons)
        {
            if (!tabButton) continue;

            tabButton.Initialize(this);

            if (tabButton.Tab != null)
            {
                tabButton.Tab.Initialize(this);
            }
        }
    }
}
