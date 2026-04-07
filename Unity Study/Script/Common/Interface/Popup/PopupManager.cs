using UnityEngine;

public class PopupManager : SingletonType<PopupManager>
{
    public Popup CreatePopup(PopupData popupData)
    {
        var root = InterfaceManager.Instance.GetUILayer(popupData.context.popupLayer);
        var popup = AssetLoader.Instance.Instantiate<Popup>(AssetLoader.POPUP_PREFIX + popupData.popupPath, parent: root);
        
        popup.InitPopup(popupData);

        return popup;
    }
}
