using Microsoft.AspNetCore.SignalR;
using Q_Server.DTOs;
using Q_Server.Services;
using System.Collections.Concurrent;

namespace Q_Server.Hubs
{
    /// <summary>
    /// 1대1 전투 실시간 통신 Hub
    /// 매칭, 전투 동기화, 결과 처리
    /// </summary>
    public class BattleHub : Hub
    {
        private readonly MatchmakingService _matchmakingService;
        
        // 활성 전투 세션 (BattleId -> 전투 정보)
        private static readonly ConcurrentDictionary<string, BattleSession> _activeBattles = new();
        
        // 플레이어 연결 매핑 (ConnectionId -> UserId)
        private static readonly ConcurrentDictionary<string, string> _playerConnections = new();

        public BattleHub(MatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        /// <summary>
        /// 매칭 대기열 참가
        /// </summary>
        public async Task JoinMatchmaking(string userId, int eloScore)
        {
            _playerConnections.TryAdd(Context.ConnectionId, userId);
            _matchmakingService.AddToQueue(Context.ConnectionId, userId, eloScore);

            // 매칭 상대 검색
            var opponent = _matchmakingService.FindMatch(Context.ConnectionId);
            
            if (opponent != null)
            {
                // 매칭 성공 - 전투 세션 생성
                var battleId = Guid.NewGuid().ToString();
                var session = new BattleSession
                {
                    BattleId = battleId,
                    Player1ConnectionId = Context.ConnectionId,
                    Player2ConnectionId = opponent.ConnectionId,
                    Player1UserId = userId,
                    Player2UserId = opponent.UserId,
                    StartedAt = DateTime.UtcNow
                };
                
                _activeBattles.TryAdd(battleId, session);
                
                // 두 플레이어를 대기열에서 제거
                _matchmakingService.RemoveFromQueue(Context.ConnectionId);
                _matchmakingService.RemoveFromQueue(opponent.ConnectionId);
                
                // 전투 그룹에 추가
                await Groups.AddToGroupAsync(Context.ConnectionId, battleId);
                await Groups.AddToGroupAsync(opponent.ConnectionId, battleId);
                
                // 매칭 완료 알림
                var matchFoundForPlayer1 = new MatchFoundDTO(
                    battleId, opponent.UserId, opponent.UserId, opponent.EloScore);
                var matchFoundForPlayer2 = new MatchFoundDTO(
                    battleId, userId, userId, eloScore);
                
                await Clients.Client(Context.ConnectionId).SendAsync("MatchFound", matchFoundForPlayer1);
                await Clients.Client(opponent.ConnectionId).SendAsync("MatchFound", matchFoundForPlayer2);
            }
            else
            {
                // 대기 중 알림
                await Clients.Caller.SendAsync("WaitingForMatch", _matchmakingService.QueueSize);
            }
        }

        /// <summary>
        /// 유닛 상태 업데이트 전송
        /// </summary>
        public async Task SendUnitUpdate(string battleId, UnitUpdateDTO unitUpdate)
        {
            if (_activeBattles.TryGetValue(battleId, out var session))
            {
                // 해당 전투 그룹의 상대방에게 전송
                await Clients.OthersInGroup(battleId).SendAsync("UnitUpdated", unitUpdate);
            }
        }

        /// <summary>
        /// 전투 액션 전송 (이동, 공격 등)
        /// </summary>
        public async Task SendBattleAction(BattleActionDTO action)
        {
            if (_activeBattles.TryGetValue(action.BattleId, out var session))
            {
                // 상대방에게 액션 전달
                await Clients.OthersInGroup(action.BattleId).SendAsync("BattleActionReceived", action);
            }
        }

        /// <summary>
        /// 전투 상태 전체 동기화
        /// </summary>
        public async Task SyncBattleState(string battleId, BattleStateDTO state)
        {
            if (_activeBattles.ContainsKey(battleId))
            {
                await Clients.Group(battleId).SendAsync("BattleStateSynced", state);
            }
        }

        /// <summary>
        /// 전투 종료 처리
        /// </summary>
        public async Task EndBattle(string battleId, string winnerId)
        {
            if (_activeBattles.TryRemove(battleId, out var session))
            {
                var loserId = winnerId == session.Player1UserId 
                    ? session.Player2UserId 
                    : session.Player1UserId;

                // ELO 변동은 실제로는 DB에서 조회 후 계산해야 함
                var (winnerChange, loserChange) = _matchmakingService.CalculateEloChange(1000, 1000);
                
                var result = new BattleResultDTO(
                    battleId, winnerId, loserId, winnerChange, loserChange);
                
                // 결과 전송
                await Clients.Group(battleId).SendAsync("BattleEnded", result);
                
                // 그룹에서 제거
                await Groups.RemoveFromGroupAsync(session.Player1ConnectionId, battleId);
                await Groups.RemoveFromGroupAsync(session.Player2ConnectionId, battleId);
            }
        }

        /// <summary>
        /// 연결 해제 시 처리
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 매칭 대기열에서 제거
            _matchmakingService.RemoveFromQueue(Context.ConnectionId);
            _playerConnections.TryRemove(Context.ConnectionId, out _);
            
            await base.OnDisconnectedAsync(exception);
        }
    }

    /// <summary>
    /// 활성 전투 세션 정보
    /// </summary>
    public class BattleSession
    {
        public string BattleId { get; set; } = string.Empty;
        public string Player1ConnectionId { get; set; } = string.Empty;
        public string Player2ConnectionId { get; set; } = string.Empty;
        public string Player1UserId { get; set; } = string.Empty;
        public string Player2UserId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
    }
}
