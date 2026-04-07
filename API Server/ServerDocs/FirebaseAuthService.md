# FirebaseAuthService

## 개요
Firebase ID 토큰을 검증하는 서비스입니다. Google의 공개 키를 사용하여 JWT 토큰을 검증합니다.

## 인터페이스

### IFirebaseAuthService
```csharp
public interface IFirebaseAuthService
{
    Task<FirebaseTokenResult> VerifyTokenAsync(string idToken);
}
```

## 클래스

### FirebaseTokenResult
토큰 검증 결과를 담는 클래스입니다.

```csharp
public class FirebaseTokenResult
{
    public bool IsValid { get; set; }
    public string? Uid { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## 설정

### appsettings.json
```json
{
  "Firebase": {
    "ProjectId": "YOUR_FIREBASE_PROJECT_ID"
  }
}
```

## 토큰 검증 과정
1. Google OpenID Connect 설정에서 서명 키 가져오기
2. 토큰의 발급자(issuer)가 `https://securetoken.google.com/{projectId}`인지 확인
3. 토큰의 대상(audience)이 Firebase 프로젝트 ID인지 확인
4. 토큰 만료 시간 확인
5. 서명 검증

## 에러 처리
- `SecurityTokenExpiredException`: 토큰 만료
- `SecurityTokenInvalidSignatureException`: 서명 불일치
- 기타 예외: 일반 검증 실패

## 등록 (Program.cs)
```csharp
builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
```
