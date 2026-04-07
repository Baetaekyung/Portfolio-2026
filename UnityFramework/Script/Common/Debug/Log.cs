using System.Text;
using UnityEngine;

public static class Log
{
    private static StringBuilder _builder = new();

    public static void WriteLog(string msg)
    {
        DebugOverride.Log(msg);
    }

    public static void WriteWarning(string msg)
    {
        _builder.Clear();
        _builder.Append("<color=#FFFF00>[WARNING] ")
            .Append(msg)
            .Append("</color>");

        DebugOverride.LogWarning(_builder.ToString());
    }

    public static void WriteError(string msg)
    {
        _builder.Clear();
        _builder.Append("<color=#FF0000>[ERROR] ")
            .Append(msg)
            .Append("</color>");

        DebugOverride.LogError(msg);
    }
}
