using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Q_Server.Data;
using Q_Server.DTOs.Generated;
using Q_Server.Models;
using Q_Server.Services;

namespace Q_Server.Controllers
{
    /// <summary>
    /// 인증 관련 API 컨트롤러
    /// Firebase 토큰 기반 인증 처리
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GameDbContext _context;
        private readonly IFirebaseAuthService _firebaseAuth;

        public AuthController(GameDbContext context, IFirebaseAuthService firebaseAuth)
        {
            _context = context;
            _firebaseAuth = firebaseAuth;
        }

        /// <summary>
        /// Firebase 토큰 인증 API
        /// 토큰 검증 후 기존 유저면 로그인, 신규 유저면 자동 회원가입
        /// </summary>
        [HttpPost("firebaseAuth")]
        public async Task<ActionResult<AuthResponseDTO>> FirebaseAuth([FromBody] FirebaseAuthRequestDTO request)
        {
            // Firebase 토큰 검증
            var tokenResult = await _firebaseAuth.VerifyTokenAsync(request.IdToken);

            if (!tokenResult.IsValid)
            {
                return Ok(new AuthResponseDTO(false, tokenResult.ErrorMessage ?? "인증 실패", 0, "", 0));
            }

            // Firebase UID로 유저 조회
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.FirebaseUid == tokenResult.Uid);

            if (user == null)
            {
                // 신규 유저 자동 생성
                user = new User
                {
                    FirebaseUid = tokenResult.Uid!,
                    Username = tokenResult.Name ?? tokenResult.Email ?? $"User_{tokenResult.Uid![..8]}",
                    EloScore = 1000,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new AuthResponseDTO(true, "회원가입 및 로그인 성공", user.Id, user.Username, user.EloScore));
            }

            // 기존 유저 로그인 처리
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDTO(true, "로그인 성공", user.Id, user.Username, user.EloScore));
        }

        /// <summary>
        /// 유저 정보 조회 API
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserInfoDTO>> GetUserInfo(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("유저를 찾을 수 없습니다.");
            }

            return Ok(new UserInfoDTO(
                user.Id,
                user.Username,
                user.EloScore,
                user.Wins,
                user.Losses
            ));
        }

        /// <summary>
        /// 유저명 변경 API
        /// Firebase 토큰 검증 후 유저명 변경
        /// </summary>
        [HttpPut("username")]
        public async Task<ActionResult<AuthResponseDTO>> UpdateUsername(
            [FromHeader(Name = "Authorization")] string authorization,
            [FromBody] UpdateUsernameRequestDTO request)
        {
            // Bearer 토큰 추출
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new AuthResponseDTO(false, "인증 토큰이 필요합니다.", 0, "", 0));
            }

            var idToken = authorization["Bearer ".Length..];
            var tokenResult = await _firebaseAuth.VerifyTokenAsync(idToken);

            if (!tokenResult.IsValid)
            {
                return Unauthorized(new AuthResponseDTO(false, tokenResult.ErrorMessage ?? "인증 실패", 0, "", 0));
            }

            // 유저 조회
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.FirebaseUid == tokenResult.Uid);

            if (user == null)
            {
                return NotFound(new AuthResponseDTO(false, "유저를 찾을 수 없습니다.", 0, "", 0));
            }

            // 유저명 중복 확인
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.NewUsername && u.Id != user.Id);

            if (existingUser != null)
            {
                return Ok(new AuthResponseDTO(false, "이미 사용 중인 유저명입니다.", 0, "", 0));
            }

            // 유저명 변경
            user.Username = request.NewUsername;
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDTO(true, "유저명 변경 성공", user.Id, user.Username, user.EloScore));
        }
    }
}
