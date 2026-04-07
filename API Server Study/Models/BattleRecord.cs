using System.ComponentModel.DataAnnotations;

namespace Q_Server.Models
{
    /// <summary>
    /// 전투 기록 엔티티
    /// 1대1 전투 결과 및 ELO 변동 기록
    /// </summary>
    public class BattleRecord
    {
        [Key]
        public int Id { get; set; }

        // 승자 유저 ID
        public int WinnerId { get; set; }

        // 패자 유저 ID
        public int LoserId { get; set; }

        // 승자 ELO 변동량
        public int WinnerEloChange { get; set; }

        // 패자 ELO 변동량
        public int LoserEloChange { get; set; }

        // 전투 시작 시간
        public DateTime StartedAt { get; set; }

        // 전투 종료 시간
        public DateTime EndedAt { get; set; } = DateTime.UtcNow;

        // 전투 지속 시간 (초)
        public int DurationSeconds { get; set; }
    }
}
