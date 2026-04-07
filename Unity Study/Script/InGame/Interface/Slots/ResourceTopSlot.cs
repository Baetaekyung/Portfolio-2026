using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceTopSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI haveCountText;
    
    public void Init(ResourceHaveData haveData)
    {
        var resourceTable = GameDataLoader.Instance.GetTable<GameResourceTable>();
        if (resourceTable == null)
        {
            Log.WriteError($"Table is valid, [GameResourceTable]");
            return;
        }
        var row = resourceTable.GetRowById(haveData.resourceIndex);
        if (row == null)
        {
            Log.WriteError($"Row is not exist at {haveData.resourceIndex}, [GameResourceTable]");
            return;
        }
        var iconPath = row.iconPath;

        var icon = AtlasManager.Instance.GetSprite(iconPath);
        if (icon == null)
        {
            Log.WriteError($"Icon is invalid, path: {iconPath}");
            return;
        }
        itemIcon.SetSprite(icon);
    }
}
