using System.ComponentModel.DataAnnotations;

namespace Q_Server.Models
{
    /// <summary>
    /// 유저 엔티티
    /// ELO 점수 기반 매칭에 사용
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // Firebase 고유 사용자 ID
        [Required]
        [MaxLength(128)]
        public string FirebaseUid { get; set; } = string.Empty;

        // ELO 점수 (기본값 1000)
        public int EloScore { get; set; } = 1000;

        // 총 승리 횟수
        public int Wins { get; set; } = 0;

        // 총 패배 횟수
        public int Losses { get; set; } = 0;

        // 계정 생성일
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 마지막 로그인
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    }
}
