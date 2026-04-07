using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using Firebase.Extensions;

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public partial class FirebaseManager : Singleton<FirebaseManager>
{
    private bool _isInitialized;
    private FirebaseAuth _auth;

    // Firebase 초기화 완료 여부
    public bool IsInitialized => _isInitialized;

    // Firebase 인증 인스턴스
    public FirebaseAuth Auth => _auth;

    // 초기화 완료 이벤트
    public event Action OnInitialized;

    protected override void OnCreated()
    {
        base.OnCreated();
        Initialize();
    }

    /*
    Firebase SDK 초기화
    의존성 확인 후 Auth 인스턴스 생성
    */
    private void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase 초기화 성공
                _auth = FirebaseAuth.DefaultInstance;
                _isInitialized = true;

                Debug.Log("[FirebaseManager] 초기화 완료");
                OnInitialized?.Invoke();
            }
            else
            {
                // 의존성 해결 실패
                Debug.LogError($"[FirebaseManager] 초기화 실패: {dependencyStatus}");
            }
        });
    }

    #region Authentication

    public event Action OnSignInSuccessed;
    public event Action<string> OnSignInFailed;

    // 현재 로그인된 사용자가 있는지 확인
    public bool HasCurrentUser => _auth?.CurrentUser != null;

    public partial void SignInAnnonymous();

    // 이전 로그인 정보로 자동 로그인 시도
    public partial bool TryAutoSignIn();

    #endregion

    #region Analize

    // 기본 이벤트 로깅
    public partial void LogEvent(string eventName);

    // 문자열 파라미터와 함께 이벤트 로깅
    public partial void LogEvent(string eventName, string paramName, string paramValue);

    // 정수 파라미터와 함께 이벤트 로깅
    public partial void LogEvent(string eventName, string paramName, long paramValue);

    // 실수 파라미터와 함께 이벤트 로깅
    public partial void LogEvent(string eventName, string paramName, double paramValue);

    // 여러 파라미터와 함께 이벤트 로깅
    public partial void LogEvent(string eventName, params Parameter[] parameters);

    // Analytics 사용자 ID 설정
    public partial void SetUserId(string userId);

    // 사용자 속성 설정
    public partial void SetUserProperty(string name, string value);

    // 화면 조회 이벤트 로깅
    public partial void LogScreenView(string screenName, string screenClass = null);

    #endregion
}
