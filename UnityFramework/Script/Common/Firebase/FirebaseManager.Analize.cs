using UnityEngine;
using Firebase.Analytics;

public partial class FirebaseManager
{
    #region Event Logging

    public partial void LogEvent(string eventName)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.LogEvent(eventName);
    }

    public partial void LogEvent(string eventName, string paramName, string paramValue)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public partial void LogEvent(string eventName, string paramName, long paramValue)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public partial void LogEvent(string eventName, string paramName, double paramValue)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public partial void LogEvent(string eventName, params Parameter[] parameters)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, parameters);
    }

    #endregion

    #region User Properties

    public partial void SetUserId(string userId)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.SetUserId(userId);
    }

    public partial void SetUserProperty(string name, string value)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        FirebaseAnalytics.SetUserProperty(name, value);
    }

    #endregion

    #region Screen Tracking

    public partial void LogScreenView(string screenName, string screenClass)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Analytics: 초기화되지 않음");
            return;
        }

        var parameters = screenClass != null
            ? new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterScreenName, screenName),
                new Parameter(FirebaseAnalytics.ParameterScreenClass, screenClass)
            }
            : new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterScreenName, screenName)
            };

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView, parameters);
    }

    #endregion
}
