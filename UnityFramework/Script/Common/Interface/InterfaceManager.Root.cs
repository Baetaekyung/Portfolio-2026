using UnityEngine;

public partial class InterfaceManager
{
    [SerializeField] private Transform safeAreaRoot;

    [SerializeField] private Transform inGameLayer;
    [SerializeField] private Transform popupLayer;
    [SerializeField] private Transform noticeLayer;
    [SerializeField] private Transform overLayer;

    public Transform SafeAreaRoot => safeAreaRoot;
        
    public Transform GetUILayer(PopupLayer layer)
    {
        return layer switch
        {
            PopupLayer.None => safeAreaRoot,
            PopupLayer.InGame => inGameLayer,
            PopupLayer.Popup => popupLayer,
            PopupLayer.Notice => noticeLayer,
            PopupLayer.Over => overLayer,
            _ => safeAreaRoot
        };
    }
}
