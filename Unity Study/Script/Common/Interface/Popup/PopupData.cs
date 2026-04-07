using UnityEngine;

public enum PopupLayer
{
    None, // 쓸 이유 거의 없을 것 같지만 우선;;
    
    InGame, // 가장 아래에 보임
    Popup,
    Notice,
    Over, // 가장 위에 보임
}

/// <summary>
/// Popup의 정보 (ex: 경로 및 Context)
/// </summary>
public class PopupData
{
    public string popupPath;
    public PopupContext context;
}

/// <summary>
/// Popup 세부 사항 (ex: Popup의 Title, description등)
/// </summary>
public abstract class PopupContext
{
    public PopupLayer popupLayer;
}