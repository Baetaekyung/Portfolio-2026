# GameDbContext.md

## 개요
MySQL 데이터베이스 컨텍스트 클래스. Entity Framework Core를 통해 게임 데이터를 관리.

## 경로
`Q_Server/Data/GameDbContext.cs`

## 주요 기능

### DbSet
- `Users` - 유저 테이블 (ELO 점수, 승/패 기록 포함)
- `BattleRecords` - 전투 기록 테이블

### 설정
- MySQL 연결 (Pomelo.EntityFrameworkCore.MySql)
- Username 유니크 인덱스
- EloScore 기본값 1000

## 사용 예시
```csharp
// DI를 통한 사용
public class AuthController : ControllerBase
{
    private readonly GameDbContext _context;
    
    public AuthController(GameDbContext context)
    {
        _context = context;
    }
}
```

## 관련 파일
- [User.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/Models/User.cs)
- [BattleRecord.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/Models/BattleRecord.cs)
