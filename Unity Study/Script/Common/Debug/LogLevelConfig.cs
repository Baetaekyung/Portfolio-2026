using UnityEngine;

public enum ELogLevel
{
    AllowDebugging, // Debug.Log부터 다 허용 (Log, Warning, Error)
    AllowWarning, // Debug.Warning부터 다 허용 (Warning, Error)
    AllowError, // Debug.Error부터 다 허용 (Error)

    AllowDebuggingOnly, // Log만
    AllowWarningOnly, // Warning만
    AllowErrorOnly, // Error만

    DebugAll, // 다 허용

    DebugNothing // 아무것도 디버그 하지 않겠다.
}

[CreateAssetMenu(fileName = "LogLevelConfig_", menuName = "Logger/LogLevelConfig")]
public class LogLevelConfig : ScriptableObject
{
    public ELogLevel LogLevel;
}
