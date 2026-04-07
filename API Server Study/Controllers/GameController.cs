using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Q_Server.Data;
using Q_Server.DTOs.Generated;
using Q_Server.Services;

namespace Q_Server.Controllers
{
    /// <summary>
    /// 게임 관련 API 컨트롤러
    /// 랭킹, 전적 조회 등
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameDbContext _context;
        private readonly MatchmakingService _matchmakingService;

        public GameController(GameDbContext context, MatchmakingService matchmakingService)
        {
            _context = context;
            _matchmakingService = matchmakingService;
        }

        /// <summary>
        /// 상위 랭커 조회 API
        /// </summary>
        [HttpGet("ranking")]
        public async Task<ActionResult<List<UserInfoDTO>>> GetRanking([FromQuery] int top = 10)
        {
            var topUsers = await _context.Users
                .OrderByDescending(u => u.EloScore)
                .Take(top)
                .Select(u => new UserInfoDTO(
                    u.Id,
                    u.Username,
                    u.EloScore,
                    u.Wins,
                    u.Losses
                ))
                .ToListAsync();

            return Ok(topUsers);
        }

        /// <summary>
        /// 유저 전적 조회 API
        /// </summary>
        [HttpGet("stats/{userId}")]
        public async Task<ActionResult<object>> GetUserStats(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("유저를 찾을 수 없습니다.");
            }

            // 최근 전투 기록 조회
            var recentBattles = await _context.BattleRecords
                .Where(b => b.WinnerId == userId || b.LoserId == userId)
                .OrderByDescending(b => b.EndedAt)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                user.EloScore,
                user.Wins,
                user.Losses,
                WinRate = user.Wins + user.Losses > 0 
                    ? (double)user.Wins / (user.Wins + user.Losses) * 100 
                    : 0,
                RecentBattles = recentBattles
            });
        }

        /// <summary>
        /// 현재 매칭 대기열 상태 조회 API
        /// </summary>
        [HttpGet("matchmaking/status")]
        public ActionResult<object> GetMatchmakingStatus()
        {
            return Ok(new
            {
                PlayersInQueue = _matchmakingService.QueueSize
            });
        }
    }
}
