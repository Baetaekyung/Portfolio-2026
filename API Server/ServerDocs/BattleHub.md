# BattleHub.md

## 개요
SignalR Hub 클래스. 1대1 실시간 전투 통신 및 ELO 기반 매칭 처리.

## 경로
`Q_Server/Hubs/BattleHub.cs`

## 주요 메서드

### 매칭
| 메서드 | 설명 |
|--------|------|
| `JoinMatchmaking(userId, eloScore)` | ELO 기반 매칭 대기열 참가 |

### 전투 동기화
| 메서드 | 설명 |
|--------|------|
| `SendUnitUpdate(battleId, UnitUpdateDTO)` | 유닛 위치/체력 동기화 |
| `SendBattleAction(BattleActionDTO)` | 이동, 공격 등 액션 전송 |
| `SyncBattleState(battleId, BattleStateDTO)` | 전체 상태 동기화 |
| `EndBattle(battleId, winnerId)` | 전투 종료 및 ELO 업데이트 |

## 클라이언트 이벤트 (수신)
- `MatchFound` - 매칭 성공
- `WaitingForMatch` - 대기 중
- `UnitUpdated` - 유닛 업데이트
- `BattleActionReceived` - 액션 수신
- `BattleStateSynced` - 상태 동기화
- `BattleEnded` - 전투 종료

## 연결 엔드포인트
```
wss://{server}/battlehub
```

## 관련 파일
- [MatchmakingService.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/Services/MatchmakingService.cs)
- [BattleDTOs.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/DTOs/BattleDTOs.cs)
