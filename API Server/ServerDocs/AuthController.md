# AuthController

## 개요
Firebase 토큰 기반 인증을 처리하는 API 컨트롤러입니다.

## 엔드포인트

### POST /api/auth/firebase
Firebase ID 토큰을 검증하고 인증을 처리합니다.

**요청**
```json
{
  "idToken": "Firebase ID 토큰"
}
```

**응답**
```json
{
  "success": true,
  "message": "로그인 성공",
  "userId": 1,
  "username": "사용자명",
  "eloScore": 1000
}
```

**동작**
- 토큰이 유효하고 기존 유저인 경우: 로그인 처리
- 토큰이 유효하고 신규 유저인 경우: 자동 회원가입 후 로그인

### GET /api/auth/user/{userId}
유저 정보를 조회합니다.

**응답**
```json
{
  "userId": 1,
  "username": "사용자명",
  "eloScore": 1000,
  "wins": 5,
  "losses": 3
}
```

### PUT /api/auth/username
유저명을 변경합니다.

**헤더**
```
Authorization: Bearer {Firebase ID 토큰}
```

**요청**
```json
{
  "newUsername": "새 유저명"
}
```

## 의존성
- `IFirebaseAuthService`: Firebase 토큰 검증 서비스
- `GameDbContext`: 데이터베이스 컨텍스트
