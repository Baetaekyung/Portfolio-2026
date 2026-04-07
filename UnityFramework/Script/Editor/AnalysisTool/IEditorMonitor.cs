/// <summary>
/// 에디터 모니터가 구현해야 하는 인터페이스
/// </summary>
public interface IEditorMonitor
{
    // 모니터 이름 (툴바에 표시)
    string MonitorName { get; }

    // 모니터 활성화 시 호출
    void OnMonitorEnable();

    // 모니터 비활성화 시 호출
    void OnMonitorDisable();

    // 매 프레임 업데이트
    void OnMonitorUpdate();

    // GUI 렌더링
    void OnMonitorGUI();
}
