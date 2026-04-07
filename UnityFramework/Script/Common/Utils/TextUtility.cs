using System.Text;
using UnityEngine;

public static class TextUtility
{
    private static StringBuilder _builder = new();

    public static string GetColorText(string originMessage, EColorEnum color)
    {
        return GetColorText(originMessage, CustomColor.GetColor(color));
    }

    public static string GetColorText(string originMessage, Color color)
    {
        _builder.Clear();
        var colorCode = ColorUtility.ToHtmlStringRGB(color);
        _builder.Append($"<color=#{colorCode}>{originMessage}</color>");
        return _builder.ToString();
    }

    /// <summary>
    /// 특정 문자를 특정 문자로 변경
    /// ex) [level] => 99
    /// </summary>
    /// <param name="originMessage"></param>
    /// <param name="originWord"></param>
    /// <param name="replacedWord"></param>
    /// <returns></returns>
    public static string GetParsedText(string originMessage, string originWord, string replacedWord)
    {
        originMessage.Replace(originWord, replacedWord);
        return originMessage;
    }
}
