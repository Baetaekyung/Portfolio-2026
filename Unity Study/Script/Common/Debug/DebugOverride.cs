using System.Diagnostics;

using Debug = UnityEngine.Debug;

public static class DebugOverride
{
    [Conditional("ENABLE_DEBUG")]
    public static void Log(string msg)
    {
        if (LogManager.Instance == null) 
            return;
        if (LogManager.Instance.DebugLog == false)
            return;

        Debug.Log(msg);
    }

    [Conditional("ENABLE_DEBUG")]
    public static void LogWarning(string msg)
    {
        if (LogManager.Instance == null)
            return;
        if (LogManager.Instance.DebugWarning == false)
            return;

        Debug.LogWarning(msg);
    }

    [Conditional("ENABLE_DEBUG")]
    public static void LogError(string msg)
    {
        if (LogManager.Instance == null)
            return;
        if (LogManager.Instance.DebugError == false)
            return;

        Debug.LogError(msg);
    }
}
