using System.Collections.Generic;
using UnityEngine;

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public partial class InterfaceManager : Singleton<InterfaceManager>
{
    private static PopupManager popupManager;
    public static PopupManager Popup => popupManager;

    [SerializeField] private GameObject background;

    private LoadingPopup _loadingPanel = null;

    private readonly Dictionary<string, GameObject> _tempUIs = new();

    protected override void OnCreated()
    {
        base.OnCreated();

        popupManager = PopupManager.Create();
    }

    public void UpdateTempUI(string key, GameObject go)
    {
        // �ߺ��Ȱ� ���� ���� ������?
        if (_tempUIs.ContainsKey(key)) return;

        _tempUIs.Add(key, go);
    }

    public void RemoveTempUI(string key)
    {
        if (_tempUIs.TryGetValue(key, out var go))
        {
            Destroy(go);
            _tempUIs.Remove(key);
        }
    }

    public void SetLoading(bool active)
    {
        if (active)
        {
            if (_loadingPanel)
            {
                var context = new LoadingPopupContext() 
                { 
                    popupLayer = PopupLayer.Over,
                };
                _loadingPanel.OnPopup(context);
            }
            else
            {
                var loadingPopupPath = "LoadingPanel.prefab";

                PopupData popupData = new PopupData();
                popupData.popupPath = loadingPopupPath;
                popupData.context = new LoadingPopupContext() 
                { 
                    popupLayer = PopupLayer.Over,
                };

                _loadingPanel = PopupManager.Instance.CreatePopup(popupData) as LoadingPopup;
                _loadingPanel.OnPopup(popupData.context);
            }
        }
        else
        {
            if (_loadingPanel)
            {
                _loadingPanel.OnPopdown();
            }
            else
            {
                Log.WriteWarning("There isn't loading popup, check the order first.");
            }
        }
    }

    public void SetBackground(bool active)
    {
        background.SetActive(active);
    }
}
