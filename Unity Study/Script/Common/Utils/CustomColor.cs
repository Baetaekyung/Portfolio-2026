using UnityEngine;

public enum EColorEnum
{
    RED,
    GREEN,
    BLUE,
    WHITE,
    BLACK,
    YELLOW
}

[CreateAssetMenu(menuName = "Scriptable/ColorPallette")]
public class CustomColorPallette : ScriptableObject
{
    [SerializeField] private UnityDictionary<EColorEnum, Color> colorPallette;

    public Color GetColor(EColorEnum colorEnum)
    {
        if (colorPallette.TryGetValue(colorEnum, out var col))
        {
            return col;
        }

        Log.WriteWarning($"[{nameof(CustomColorPallette)}] {colorEnum} color doesn't exist.");
        return Color.yellow;
    }
}

public static class CustomColor
{
    public static CustomColorPallette Pallette;

    private static void Initialize()
    {
        if (Pallette != null) return;

        Pallette = Resources.Load<CustomColorPallette>("ColorPallette");
    }

    public static Color GetColor(EColorEnum color)
    {
        Initialize();
        return Pallette.GetColor(color);
    }

    public static string GetColorCode(EColorEnum colorEnum)
    {
        Initialize();
        var color = GetColor(colorEnum);
        return ColorUtility.ToHtmlStringRGB(color);
    }
}
