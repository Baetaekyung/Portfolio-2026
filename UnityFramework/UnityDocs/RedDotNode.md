# RedDotNode

## 개요
RedDot 트리의 개별 노드를 나타내는 순수 C# 클래스입니다.
리프 노드는 조건 함수(`Func<bool>`)로 상태를 판단하고, 비-리프 노드는 자식 상태를 집계(OR)하여 판단합니다.

## 클래스 정보
- **위치**: `Assets/Script/InGame/Interface/RedDot/RedDotNode.cs`
- **상속**: 없음 (순수 C# 클래스)
- **의존성**: `ERedDotKey`

## 노드 유형

| 유형 | 조건 | 판단 방식 |
|------|------|----------|
| 리프 노드 (Leaf) | `children`이 없음 | `condition()` 실행 결과 |
| 비-리프 노드 (Branch) | `children`이 있음 | 자식 중 하나라도 활성이면 활성 |

## 핵심 멤버

### 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---------|------|------|
| `Key` | `ERedDotKey` | 노드 식별 키 |
| `Parent` | `RedDotNode` | 부모 노드 (루트는 null) |
| `IsActive` | `bool` | 현재 활성 상태 |
| `IsDirty` | `bool` | 재평가 필요 여부 |
| `IsLeaf` | `bool` | 리프 노드 여부 |
| `Depth` | `int` | 트리 깊이 (루트 = 0) |

### 메서드

| 메서드 | 설명 |
|--------|------|
| `SetCondition(Func<bool>)` | 리프 노드용 조건 함수 등록 |
| `AddListener(Action<bool>)` | UI 바인딩용 리스너 등록 (등록 즉시 현재 상태 알림) |
| `RemoveListener(Action<bool>)` | UI 바인딩 해제 |
| `SetDirty(bool)` | Dirty 상태 설정 |
| `Evaluate()` | 노드 상태 평가 (상태 변경 시 리스너에 알림) |

## 평가 로직

### 리프 노드
```
Evaluate():
    newValue = condition != null && condition()
    if newValue != isActive:
        isActive = newValue
        onValueChanged(isActive)    ← UI에 알림
```

### 비-리프 노드
```
Evaluate():
    newValue = children 중 하나라도 isActive == true
    if newValue != isActive:
        isActive = newValue
        onValueChanged(isActive)    ← UI에 알림
```

## 트리 구조 예시

```
ROOT (depth=0, Branch)
├── SHOP (depth=1, Branch)
│   ├── SHOP_WEAPON (depth=2, Leaf) → condition: 구매 가능 무기 존재?
│   └── SHOP_ARMOR  (depth=2, Leaf) → condition: 구매 가능 방어구 존재?
├── MAIL (depth=1, Leaf)            → condition: 읽지 않은 메일 존재?
└── CHARACTER (depth=1, Branch)
    ├── CHARACTER_EQUIPMENT (depth=2, Leaf) → condition: 더 좋은 장비 존재?
    └── CHARACTER_SKILL    (depth=2, Leaf)  → condition: 배울 수 있는 스킬 존재?
```

## 주의사항

- `RedDotNode`는 직접 생성하지 않고, `RedDotConditionManager.InitTree()`에서 생성합니다
- `Evaluate()`는 `RedDotConditionManager`의 `LateUpdate`에서 호출됩니다
- 리스너 등록 시 즉시 현재 상태를 콜백하므로, UI 초기 상태 설정이 자동으로 처리됩니다
- 상태가 변경되지 않으면 리스너에 알리지 않아 불필요한 UI 갱신을 방지합니다
