using Microsoft.EntityFrameworkCore;
using Q_Server.Models;

namespace Q_Server.Data
{
    /// <summary>
    /// MySQL 데이터베이스 컨텍스트
    /// EF Core를 통해 게임 데이터를 관리
    /// </summary>
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        // 유저 테이블
        public DbSet<User> Users { get; set; }

        // 전투 기록 테이블
        public DbSet<BattleRecord> BattleRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 엔티티 설정
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.EloScore).HasDefaultValue(1000);
            });

            // BattleRecord 엔티티 설정
            modelBuilder.Entity<BattleRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.WinnerId);
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.LoserId);
            });
        }
    }
}
