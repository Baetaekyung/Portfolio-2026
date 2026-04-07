using UnityEngine;
using Firebase.Extensions;

public partial class FirebaseManager
{
    /*
    이전 로그인 정보로 자동 로그인 시도
    성공 시 true 반환 및 OnSignInSuccessed 이벤트 발생
    */
    public partial bool TryAutoSignIn()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] 초기화되지 않음. 자동 로그인 불가");
            return false;
        }

        var currentUser = _auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.Log("[FirebaseManager] 저장된 로그인 정보 없음");
            return false;
        }

        // 토큰 갱신하여 유효성 확인
        currentUser.ReloadAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("[FirebaseManager] 자동 로그인 실패: 토큰 만료");
                OnSignInFailed?.Invoke("Token expired");
                return;
            }

            Debug.Log($"[FirebaseManager] 자동 로그인 성공: {currentUser.UserId}");
            OnSignInSuccessed?.Invoke();

            LogEvent("auth-auto");
        });

        return true;
    }

    // 익명 로그인 수행
    public partial void SignInAnnonymous()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] 초기화되지 않음. 로그인 불가");
            return;
        }

        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("[FirebaseManager] 익명 로그인 취소됨");
                OnSignInFailed?.Invoke("Login canceled");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseManager] 익명 로그인 실패: {task.Exception}");
                OnSignInFailed?.Invoke("[Error] Login failed");
                return;
            }

            var result = task.Result;
            Debug.Log($"[FirebaseManager] 익명 로그인 성공: {result.User.UserId}");
            OnSignInSuccessed?.Invoke();

            LogEvent("auth-annonymous");
        });
    }
}
