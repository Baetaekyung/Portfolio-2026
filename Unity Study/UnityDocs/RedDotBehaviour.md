# RedDotBehaviour

## 개요
RedDot UI 표시를 담당하는 MonoBehaviour 컴포넌트입니다.
Inspector에서 키와 대상 오브젝트를 설정하면, 코드 작성 없이 RedDot 연동이 완료됩니다.

## 클래스 정보
- **위치**: `Assets/Script/InGame/Interface/RedDot/RedDotBehaviour.cs`
- **상속**: `MonoBehaviour`
- **의존성**: `RedDotConditionManager`, `ERedDotKey`

## Inspector 설정

| 필드 | 타입 | 설명 |
|------|------|------|
| `redDotKey` | `ERedDotKey` | 구독할 RedDot 노드 키 |
| `redDotObject` | `GameObject` | 표시/숨김할 빨간 점 오브젝트 |

## 동작 방식

```
OnEnable  → RedDotConditionManager에 리스너 등록
            → 등록 즉시 현재 상태 반영

상태 변경  → OnRedDotChanged 콜백 수신
            → redDotObject.SetActive(isActive)

OnDisable → RedDotConditionManager에서 리스너 해제
```

## 설정 방법

### 1. 버튼에 RedDot 추가

```
ShopButton (Button)
├── ButtonIcon
├── ButtonText
├── RedDot (RedDot.prefab 인스턴스)    ← redDotObject로 지정
└── RedDotBehaviour 컴포넌트           ← ShopButton에 추가
    ├── redDotKey: SHOP
    └── redDotObject: RedDot
```

### 2. 설정 순서
1. 대상 버튼에 `RedDotBehaviour` 컴포넌트를 추가합니다
2. `RedDot.prefab`을 버튼의 자식으로 배치합니다
3. Inspector에서 `redDotKey`를 원하는 키로 설정합니다
4. Inspector에서 `redDotObject`에 RedDot 프리팹 인스턴스를 드래그합니다

## 사용 예시

### Inspector 기반 (권장)
코드 작성이 필요 없습니다. Inspector에서 `redDotKey`와 `redDotObject`만 설정하면 됩니다.

### 코드 기반 (동적 생성 시)
```csharp
var behaviour = button.AddComponent<RedDotBehaviour>();
// SerializeField는 코드에서 직접 설정 불가
// 동적 생성이 필요하면 RedDotConditionManager.AddListener를 직접 사용
```

### 코드에서 직접 리스너 등록 (RedDotBehaviour 없이)
```csharp
void OnEnable()
{
    RedDotConditionManager.Instance.AddListener(ERedDotKey.MAIL, OnMailRedDot);
}

void OnDisable()
{
    RedDotConditionManager.Instance.RemoveListener(ERedDotKey.MAIL, OnMailRedDot);
}

void OnMailRedDot(bool isActive)
{
    // 커스텀 UI 처리
}
```

## 주의사항

- `RedDotBehaviour`는 항상 활성 상태인 오브젝트에 추가해야 합니다
- `redDotObject`를 자기 자신으로 설정하면 비활성화 시 `OnDisable`이 호출되어 리스너가 해제되므로, 다시 활성화되지 않습니다
- `redDotObject`가 null이면 상태 변경이 무시됩니다
- 같은 `ERedDotKey`를 여러 `RedDotBehaviour`에서 사용할 수 있습니다
