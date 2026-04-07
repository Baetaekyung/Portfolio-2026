# MatchmakingService.md

## 개요
ELO 점수 기반 매칭 서비스. 유사한 ELO의 플레이어를 매칭.

## 경로
`Q_Server/Services/MatchmakingService.cs`

## 매칭 알고리즘

### ELO 범위
- 기본 범위: ±100
- 대기 시간당 확장: 초당 +10
- 최대 범위: ±500

### ELO 변동 계산
- K-Factor: 32
- 예상 승률 기반 계산

```csharp
// 승자 ELO 변동
int winnerChange = K * (1 - expectedWinner);

// 패자 ELO 변동
int loserChange = -winnerChange;
```

## 주요 메서드

| 메서드 | 설명 |
|--------|------|
| `AddToQueue(connectionId, userId, eloScore)` | 대기열 추가 |
| `RemoveFromQueue(connectionId)` | 대기열 제거 |
| `FindMatch(connectionId)` | 적합한 상대 검색 |
| `CalculateEloChange(winnerElo, loserElo)` | ELO 변동 계산 |

## 관련 파일
- [BattleHub.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/Hubs/BattleHub.cs)
