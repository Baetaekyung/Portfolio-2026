# ParallaxBehaviour

다중 레이어 패럴랙스 배경을 관리하는 컴포넌트입니다.

## 개요

- **위치**: `Assets/Script/InGame/ParallaxBehaviour.cs`
- **상속**: `MonoBehaviour`
- **의존성**: 없음

앞/중간/뒤 3개 레이어에 Transform만 할당하면 자동으로 무한 스크롤 배경을 구현합니다.
각 레이어는 독립적인 속도를 가지며, 카메라 경계 기준으로 오브젝트를 자동 생성/제거합니다.

---

## 이동 상태 (BackgroundMovementState)

| Enum 값 | 설명 |
|---------|------|
| `MOVING` | 배경이 이동 중 |
| `STOPPED` | 배경이 정지됨 |

---

## Inspector 설정

### 레이어 소스 (Transform만 할당)

| 변수명 | 타입 | 설명 |
|--------|------|------|
| `backLayer` | `Transform` | 뒤쪽 배경 오브젝트들의 부모 Transform |
| `middleLayer` | `Transform` | 중간 배경 오브젝트들의 부모 Transform |
| `frontLayer` | `Transform` | 앞쪽 배경 오브젝트들의 부모 Transform |

> null인 레이어는 자동으로 건너뜁니다. 2개만 사용해도 됩니다.

### 속도 설정

| 변수명 | 타입 | 기본값 | 설명 |
|--------|------|--------|------|
| `backSpeed` | `float` | 0.2 | 뒤쪽 레이어 속도 |
| `middleSpeed` | `float` | 0.5 | 중간 레이어 속도 |
| `frontSpeed` | `float` | 1.0 | 앞쪽 레이어 속도 |

### 공통 설정

| 변수명 | 타입 | 기본값 | 설명 |
|--------|------|--------|------|
| `direction` | `Vector2` | `Vector2.left` | 스크롤 방향 |
| `speedMultiplier` | `float` | 1 | 글로벌 속도 배율 |
| `minSeparation` | `float` | 0 | 오브젝트 간 최소 간격 |
| `maxSeparation` | `float` | 0 | 오브젝트 간 최대 간격 |

---

## Public 메서드

| 메서드 | 설명 |
|--------|------|
| `StopParallax()` | 패럴랙스 정지 (speedMultiplier = 0) |
| `ResumeParallax()` | 원래 속도로 패럴랙스 재개 |
| `SimulateParallax(float simulationSpeed)` | 지정한 속도로 패럴랙스 시뮬레이션 |

## 프로퍼티 (읽기 전용)

| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| `MovementState` | `BackgroundMovementState` | 현재 이동 상태 |
| `IsMoving` | `bool` | 현재 이동 중인지 여부 |

## 이벤트

| 이벤트 | 타입 | 설명 |
|--------|------|------|
| `OnMovementStateChanged` | `static Action<BackgroundMovementState>` | 이동 상태 변경 시 발생 |

---

## 설정 방법

### 하이어라키 구조

```
ParallaxBehaviour (GameObject)
├── BackObjects (backLayer에 할당)
│   ├── Cloud1
│   ├── Cloud2
│   └── Mountain1
├── MiddleObjects (middleLayer에 할당)
│   ├── Tree1
│   └── Tree2
└── FrontObjects (frontLayer에 할당)
    ├── Grass1
    └── Grass2
```

### 설정 순서

1. 빈 GameObject 생성 후 `ParallaxBehaviour` 컴포넌트 추가
2. 각 레이어용 빈 GameObject를 자식으로 생성 (BackObjects, MiddleObjects, FrontObjects)
3. 각 컨테이너 하위에 스프라이트 오브젝트 배치
4. Inspector에서 `backLayer`, `middleLayer`, `frontLayer`에 각 컨테이너 할당
5. 속도 조정 (기본값이 이미 자연스러운 비율로 설정됨)

---

## 사용 예시

### 기본 제어

```csharp
var parallax = GetComponent<ParallaxBehaviour>();

// 패럴랙스 시작
parallax.ResumeParallax();

// 패럴랙스 정지
parallax.StopParallax();

// 2배속 시뮬레이션
parallax.SimulateParallax(2.0f);
```

### 이벤트 구독

```csharp
private void OnEnable()
{
    ParallaxBehaviour.OnMovementStateChanged += HandleMovementChange;
}

private void OnDisable()
{
    ParallaxBehaviour.OnMovementStateChanged -= HandleMovementChange;
}

private void HandleMovementChange(BackgroundMovementState newState)
{
    if (newState == BackgroundMovementState.MOVING)
    {
        // 배경 이동 시작 처리
    }
}
```

---

## 주의사항

- 소스 오브젝트는 `Start()` 시 비활성화됨 (템플릿 역할)
- 카메라는 **Orthographic** 모드 필수
- `direction`이 `Vector2.zero`이면 이동하지 않음
- null인 레이어는 자동으로 무시됨 (3개 모두 필수가 아님)
- 너비 측정은 SpriteRenderer 기반 자동 측정 (스케일 반영)
