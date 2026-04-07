namespace Q_Server.DTOs
{
    /// <summary>
    /// 유닛 상태 업데이트 DTO
    /// 맵에 존재하는 유닛의 위치와 체력 동기화
    /// </summary>
    public record UnitUpdateDTO(
        string UnitId,
        float PositionX,
        float PositionY,
        float PositionZ,
        int Health,
        int MaxHealth
    );

    /// <summary>
    /// 전투 상태 DTO
    /// 현재 전투 전체 상태 동기화
    /// </summary>
    public record BattleStateDTO(
        string BattleId,
        List<UnitUpdateDTO> Units,
        int CurrentTurn,
        string CurrentPlayerId
    );

    /// <summary>
    /// 전투 결과 DTO
    /// 전투 종료 시 결과 및 ELO 변동 전달
    /// </summary>
    public record BattleResultDTO(
        string BattleId,
        string WinnerId,
        string LoserId,
        int WinnerEloChange,
        int LoserEloChange
    );

    /// <summary>
    /// 매칭 요청 DTO
    /// 매칭 대기열 참가 요청
    /// </summary>
    public record MatchRequestDTO(
        string UserId,
        int EloScore
    );

    /// <summary>
    /// 매칭 완료 DTO
    /// 매칭 성공 시 상대 정보 전달
    /// </summary>
    public record MatchFoundDTO(
        string BattleId,
        string OpponentId,
        string OpponentName,
        int OpponentElo
    );

    /// <summary>
    /// 전투 액션 DTO
    /// 유닛 이동, 공격 등 액션 전송
    /// </summary>
    public record BattleActionDTO(
        string BattleId,
        string PlayerId,
        string ActionType,
        string UnitId,
        float TargetX,
        float TargetY,
        float TargetZ,
        string? TargetUnitId
    );
}
