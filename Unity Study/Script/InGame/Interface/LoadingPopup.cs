using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPopupContext : PopupContext
{
    // now.. hmm.. empty..
}

public class LoadingPopup : Popup
{
    public const int MAX_MESSAGE_COUNT = 10;

    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Image characterIllust;
    [SerializeField] private CanvasGroup cg;

    // override reason: Loading can see immediatly
    public override void OnPopup(PopupContext context)
    {
        cg.alpha = 1f;

        SetLoadingPanel((LoadingPopupContext)context);
    }

    // override reason: Loading can see immediatly
    public override void OnPopdown()
    {
        cg.alpha = 0f;
    }

    private void SetLoadingPanel(LoadingPopupContext context)
    {
        var loadingData = context;
        if (loadingData == null)
        {
            Log.WriteError("It need loadingPopupData but you put just PopupData");
            return;
        }

        var table = GameDataLoader.Instance.GetTable<CharacterConceptTable>();
        if (table == null)
        {
            Log.WriteError("CharacterConceptTable doesn't exist");
            return;
        }

        int index = Random.Range(0, table.Rows.Count);
        var row = table.GetRowById(index + 1);
        if (row == null)
        {
            Log.WriteError($"CharacterConceptTable {index} row doesn't exist.");
            return;
        }

        var illust = AtlasManager.Instance.GetSprite(AtlasManager.ILLUST_ATLAS, row.path);
        if (illust == null)
        {
            Log.WriteError($"Doesn't exist illust at {row.path}");
            return;
        }

        loadingText.SetText(row.shortDesc);
        characterIllust.SetSprite(illust);
        characterNameText.SetText($"{row.characterName} - {row.title}");
    }
}
