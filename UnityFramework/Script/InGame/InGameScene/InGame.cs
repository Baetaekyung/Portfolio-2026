using UnityEngine;

public class InGame : MonoBehaviour
{
    private void Awake()
    {
        InterfaceManager.Instance.SetBackground(false);
        InterfaceManager.Instance.SetLoading(false);
        InterfaceManager.Instance.RemoveTempUI("Title");

        AssetLoader.Instance.InstantiateAsync<MainUI>(
            AssetLoader.INTERFACE_PREFIX + "InGame_Main.prefab",
            parent: InterfaceManager.Instance.GetUILayer(PopupLayer.InGame));
    }
}
