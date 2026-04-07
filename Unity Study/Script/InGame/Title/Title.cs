using UnityEngine;

public class Title : MonoBehaviour
{
    private const string TITLE_UI_PATH = "Title.prefab";

    private void Start()
    {
        var path = AssetLoader.INTERFACE_PREFIX + TITLE_UI_PATH;

        AssetLoader.Instance.InstantiateAsync(
            key: path, 
            parent: InterfaceManager.Instance.GetUILayer(PopupLayer.InGame),
            onComplete: (instance) =>
            {
                InterfaceManager.Instance.UpdateTempUI("Title", instance);
            });
    }
}
