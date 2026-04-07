namespace Q_Server.DTOs.Definitions
{
    /*
     * DTO 정의 인터페이스
     * 이 인터페이스들을 기반으로 DTOGenerator가
     * Server용 record와 Unity용 class를 자동 생성
     */

    /// <summary>
    /// Firebase 토큰 인증 요청 DTO 정의
    /// 클라이언트에서 Firebase 로그인 후 받은 ID 토큰 전송
    /// </summary>
    public interface IFirebaseAuthRequestDTO
    {
        string IdToken { get; }
    }

    /// <summary>
    /// 인증 응답 DTO 정의
    /// </summary>
    public interface IAuthResponseDTO
    {
        bool Success { get; }
        string? Message { get; }
        int? UserId { get; }
        string? Username { get; }
        int? EloScore { get; }
    }

    /// <summary>
    /// 유저명 변경 요청 DTO 정의
    /// </summary>
    public interface IUpdateUsernameRequestDTO
    {
        string NewUsername { get; }
    }

    /// <summary>
    /// 유저 정보 DTO 정의
    /// </summary>
    public interface IUserInfoDTO
    {
        int UserId { get; }
        string Username { get; }
        int EloScore { get; }
        int Wins { get; }
        int Losses { get; }
    }

    /// <summary>
    /// 유닛 업데이트 DTO 정의
    /// </summary>
    public interface IUnitUpdateDTO
    {
        string UnitId { get; }
        float PositionX { get; }
        float PositionY { get; }
        float PositionZ { get; }
        int Health { get; }
        int MaxHealth { get; }
    }

    /// <summary>
    /// 전투 상태 DTO 정의
    /// </summary>
    public interface IBattleStateDTO
    {
        string BattleId { get; }
        int CurrentTurn { get; }
        string CurrentPlayerId { get; }
    }

    /// <summary>
    /// 전투 결과 DTO 정의
    /// </summary>
    public interface IBattleResultDTO
    {
        string BattleId { get; }
        string WinnerId { get; }
        string LoserId { get; }
        int WinnerEloChange { get; }
        int LoserEloChange { get; }
    }

    /// <summary>
    /// 매칭 요청 DTO 정의
    /// </summary>
    public interface IMatchRequestDTO
    {
        string UserId { get; }
        int EloScore { get; }
    }

    /// <summary>
    /// 매칭 완료 DTO 정의
    /// </summary>
    public interface IMatchFoundDTO
    {
        string BattleId { get; }
        string OpponentId { get; }
        string OpponentName { get; }
        int OpponentElo { get; }
    }

    /// <summary>
    /// 전투 액션 DTO 정의
    /// </summary>
    public interface IBattleActionDTO
    {
        string BattleId { get; }
        string PlayerId { get; }
        string ActionType { get; }
        string UnitId { get; }
        float TargetX { get; }
        float TargetY { get; }
        float TargetZ { get; }
        string? TargetUnitId { get; }
    }
}
