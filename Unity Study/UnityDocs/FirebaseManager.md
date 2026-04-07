# FirebaseManager

## 개요
Firebase SDK 초기화 및 인증 기능을 관리하는 싱글톤 클래스입니다.

## 클래스 구조

### FirebaseManager (partial class)
- `FirebaseManager.cs`: 초기화 및 핵심 로직
- `FirebaseManager.Auth.cs`: 인증 관련 로직
- `FirebaseManager.Analize.cs`: Analytics 이벤트 로깅

## 속성

| 이름 | 타입 | 설명 |
|------|------|------|
| IsInitialized | bool | Firebase 초기화 완료 여부 |
| Auth | FirebaseAuth | Firebase 인증 인스턴스 |
| HasCurrentUser | bool | 현재 로그인된 사용자 존재 여부 |

## 이벤트

| 이름 | 타입 | 설명 |
|------|------|------|
| OnInitialized | Action | Firebase 초기화 완료 시 호출 |
| OnSignInSuccessed | Action | 로그인 성공 시 호출 |
| OnSignInFailed | Action\<string\> | 로그인 실패 시 호출 (에러 메시지 전달) |

## 메서드

### Initialize()
Firebase SDK를 초기화합니다.
- `CheckAndFixDependenciesAsync()`로 의존성 확인
- 초기화 성공 시 `OnInitialized` 이벤트 발생
- 실패 시 에러 로그 출력

### SignInAnnonymous()
익명 로그인을 수행합니다.
- 초기화 완료 후에만 동작
- 성공 시 `OnSignInSuccessed` 이벤트 발생

### TryAutoSignIn()
저장된 로그인 정보로 자동 로그인을 시도합니다.
- Firebase가 내부적으로 유지하는 CurrentUser 정보 활용
- 저장된 정보가 있으면 토큰 갱신 후 `OnSignInSuccessed` 이벤트 발생
- 저장된 정보가 없으면 false 반환
- 토큰 만료 시 `OnSignInFailed` 이벤트 발생

## 사용 예시

```csharp
// 초기화 완료 대기
FirebaseManager.Instance.OnInitialized += () =>
{
    // 자동 로그인 시도, 실패 시 익명 로그인
    if (!FirebaseManager.Instance.TryAutoSignIn())
    {
        FirebaseManager.Instance.SignInAnnonymous();
    }
};

// 로그인 성공 핸들러
FirebaseManager.Instance.OnSignInSuccessed += () =>
{
    var user = FirebaseManager.Instance.Auth.CurrentUser;
    Debug.Log($"로그인 성공: {user.UserId}");
};

// 로그인 실패 핸들러
FirebaseManager.Instance.OnSignInFailed += (message) =>
{
    Debug.LogError($"로그인 실패: {message}");
};
```

## Analytics 메서드

### LogEvent(string eventName)
기본 이벤트를 로깅합니다.

### LogEvent(string eventName, string paramName, string paramValue)
문자열 파라미터와 함께 이벤트를 로깅합니다.

### LogEvent(string eventName, string paramName, long paramValue)
정수 파라미터와 함께 이벤트를 로깅합니다.

### LogEvent(string eventName, string paramName, double paramValue)
실수 파라미터와 함께 이벤트를 로깅합니다.

### LogEvent(string eventName, params Parameter[] parameters)
여러 파라미터와 함께 이벤트를 로깅합니다.

### SetUserId(string userId)
Analytics 사용자 ID를 설정합니다.

### SetUserProperty(string name, string value)
사용자 속성을 설정합니다.

### LogScreenView(string screenName, string screenClass = null)
화면 조회 이벤트를 로깅합니다.

## Analytics 사용 예시

```csharp
// 기본 이벤트
FirebaseManager.Instance.LogEvent("game_start");

// 파라미터와 함께
FirebaseManager.Instance.LogEvent("level_complete", "level", 5);
FirebaseManager.Instance.LogEvent("purchase", "item_name", "sword");

// 여러 파라미터
FirebaseManager.Instance.LogEvent("battle_end",
    new Parameter("result", "win"),
    new Parameter("score", 1500),
    new Parameter("duration", 120.5)
);

// 사용자 속성
FirebaseManager.Instance.SetUserId("user_123");
FirebaseManager.Instance.SetUserProperty("player_level", "10");

// 화면 추적
FirebaseManager.Instance.LogScreenView("MainMenu", "MainMenuScreen");
```

## 의존성
- Firebase.Auth
- Firebase.Analytics
- Firebase.Extensions (TaskExtension)
