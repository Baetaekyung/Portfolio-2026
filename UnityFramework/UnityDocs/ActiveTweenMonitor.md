# ActiveTweenMonitor

실행 중인 TweenBehaviour들을 모니터링하는 에디터 모니터입니다.

## 개요

씬 내의 모든 TweenBehaviour 컴포넌트를 실시간으로 추적하고 상태를 표시합니다. IEditorMonitor 인터페이스를 구현하여 EditorMonitorManager에서 자동으로 등록됩니다.

## 기능

### 모니터링 정보
| 항목 | 설명 |
|------|------|
| 오브젝트 | TweenBehaviour가 붙은 GameObject (클릭 시 선택) |
| 타입 | 애니메이션 타입 (BOBBING, SHAKE 등) |
| 상태 | 재생 중 / 정지 |
| Duration | 애니메이션 한 사이클 시간 |
| Strength | 애니메이션 강도 |
| Loop | 반복 여부 |

### UI 기능
- **재생 중인 Tween만 표시**: 토글을 통해 현재 재생 중인 Tween만 필터링
- **통계 표시**: 전체 TweenBehaviour 수와 재생 중인 수 표시
- **오브젝트 선택**: 목록에서 항목 클릭 시 해당 GameObject 선택 및 Hierarchy에서 하이라이트
- **재생/정지 제어**: 개별 Tween의 재생/정지 버튼

## 사용 방법

1. Unity 에디터에서 `CustomAnalize > MonitorManager` 메뉴 클릭
2. 툴바에서 "Active Tween Monitor" 선택
3. 플레이 모드로 진입
4. 씬 내의 TweenBehaviour들이 자동으로 목록에 표시됨

## 구현 세부사항

- **업데이트 주기**: 0.5초마다 씬의 TweenBehaviour 목록 갱신
- **플레이 모드 전용**: 에디터 모드에서는 동작하지 않음
- **재생 중 항목 강조**: 녹색 배경으로 표시

## 의존성

- TweenBehaviour
- IEditorMonitor
- EditorMonitorManager
