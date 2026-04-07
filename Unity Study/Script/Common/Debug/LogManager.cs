using UnityEngine;

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class LogManager : Singleton<LogManager>
{
    [SerializeField] private LogLevelConfig logLevelConfig;

    private bool _canLogDebugLog = true;
    private bool _canLogDebugWarning = true;
    private bool _canLogDebugError = true;

    public bool DebugLog => _canLogDebugLog;
    public bool DebugWarning => _canLogDebugWarning;
    public bool DebugError => _canLogDebugError;

    protected override void Awake()
    {
        base.Awake();

        var invalid = !Validator.IsValidReferences(out var log, logLevelConfig);
        if (invalid)
        {
            Debug.LogError(log);
            return;
        }

        SetLogLevel(logLevelConfig);
    }

    public void SetLogLevel(LogLevelConfig config)
    {
        // 일단 기본으로 true
        _canLogDebugLog = true;
        _canLogDebugWarning = true;
        _canLogDebugError = true;

        switch (config.LogLevel)
        {
            case ELogLevel.AllowDebugging:
                break;

            case ELogLevel.AllowWarning:
                _canLogDebugLog = false;
                break;
            
            case ELogLevel.AllowError:
                _canLogDebugLog = false;
                _canLogDebugWarning = false;
                break;
            
            case ELogLevel.AllowDebuggingOnly:
                _canLogDebugWarning = false;
                _canLogDebugError = false;
                break;
            
            case ELogLevel.AllowWarningOnly:
                _canLogDebugLog = false;
                _canLogDebugError = false;
                break;
            
            case ELogLevel.AllowErrorOnly:
                _canLogDebugLog = false;
                _canLogDebugWarning = false;
                break;
            
            case ELogLevel.DebugAll:
                break;
            
            case ELogLevel.DebugNothing:
                _canLogDebugLog = false;
                _canLogDebugWarning = false;
                _canLogDebugError = false;
                break;
        }
    }
}
