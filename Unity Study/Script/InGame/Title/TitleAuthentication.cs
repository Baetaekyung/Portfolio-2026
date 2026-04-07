using System;
using UnityEngine;

public class TitleAuthentication : MonoBehaviour
{
    [SerializeField] private TitleIntro intro;
    [SerializeField] private GameObject signInInterface;

    private bool _tryingSignIn = false;

    // 수동 로그인이 필요한 상태인지 여부

    private void OnEnable()
    {
        signInInterface.SetActive(false);
        FirebaseManager.Instance.OnSignInSuccessed += OnSignedSuccessedHandler;
        FirebaseManager.Instance.OnSignInFailed += OnSignedFailedHandler;

        // Firebase 초기화 완료 시 자동 로그인 시도
        if (FirebaseManager.Instance.IsInitialized)
        {
            TryAutoSignIn();
        }
        else
        {
            FirebaseManager.Instance.OnInitialized += OnFirebaseInitializedHandler;
        }
    }

    private void OnDisable()
    {
        FirebaseManager.Instance.OnSignInSuccessed -= OnSignedSuccessedHandler;
        FirebaseManager.Instance.OnSignInFailed -= OnSignedFailedHandler;
        FirebaseManager.Instance.OnInitialized -= OnFirebaseInitializedHandler;
    }

    // Firebase 초기화 완료 시 자동 로그인 시도
    private void OnFirebaseInitializedHandler()
    {
        FirebaseManager.Instance.OnInitialized -= OnFirebaseInitializedHandler;
        TryAutoSignIn();
    }

    // 저장된 로그인 정보로 자동 로그인 시도
    private void TryAutoSignIn()
    {
        if (_tryingSignIn) return;
        InterfaceManager.Instance.SetLoading(true);

        if (FirebaseManager.Instance.TryAutoSignIn())
        {
            _tryingSignIn = true;
            InterfaceManager.Instance.SetLoading(false);
        }
        else
        {
            // 자동 로그인 불가, 수동 로그인 필요
            signInInterface.SetActive(true);
            InterfaceManager.Instance.SetLoading(false);
        }
    }

    private void OnSignedSuccessedHandler()
    {
        gameObject.SetActive(false);
        intro.gameObject.SetActive(true);

        _tryingSignIn = false;
    }

    private void OnSignedFailedHandler(string failMessage)
    {
        _tryingSignIn = false;
    }

    public void SignInAnnonymousHandler()
    {
        if (_tryingSignIn) return;

        FirebaseManager.Instance.SignInAnnonymous();
        _tryingSignIn = true;
    }
}
