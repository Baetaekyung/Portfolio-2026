using UnityEngine;

public abstract class Tab : MonoBehaviour
{
    public abstract void Initialize(TabController controller);
    public abstract void SelectTab();
}
