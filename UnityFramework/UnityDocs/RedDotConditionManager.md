# RedDotConditionManager

## 개요
RedDot 트리 구조를 관리하는 중앙 매니저입니다.
Dirty Flag 기반으로 `LateUpdate`에서 변경된 노드만 일괄 평가하여 성능을 최적화합니다.

## 클래스 정보
- **위치**: `Assets/Script/InGame/Interface/RedDot/RedDotConditionManager.cs`
- **상속**: `Singleton<RedDotConditionManager>`
- **의존성**: `RedDotNode`, `ERedDotKey`

## 데이터 구조

### ERedDotKey
```csharp
public enum ERedDotKey
{
    ROOT,
    // 필요에 따라 추가
}
```

### 내부 구조
| 필드 | 타입 | 설명 |
|------|------|------|
| `_nodeMap` | `Dictionary<ERedDotKey, RedDotNode>` | Key로 노드 빠른 접근 |
| `_dirtyNodes` | `HashSet<RedDotNode>` | 재평가 필요한 노드 집합 (중복 방지) |
| `_sortBuffer` | `List<RedDotNode>` | LateUpdate 정렬용 버퍼 |
| `_root` | `RedDotNode` | 트리 루트 노드 |

## 트리 구조 정의 방법

`InitTree()` 메서드에서 `CreateNode`를 호출하여 부모-자식 관계를 정의합니다.

### 1단계: ERedDotKey에 키 추가
```csharp
public enum ERedDotKey
{
    ROOT,
    SHOP,
    SHOP_WEAPON,
    SHOP_ARMOR,
    MAIL,
}
```

### 2단계: InitTree()에서 트리 구성
```csharp
private void InitTree()
{
    _root = CreateNode(ERedDotKey.ROOT);

    CreateNode(ERedDotKey.SHOP, ERedDotKey.ROOT);
    CreateNode(ERedDotKey.SHOP_WEAPON, ERedDotKey.SHOP);
    CreateNode(ERedDotKey.SHOP_ARMOR, ERedDotKey.SHOP);
    CreateNode(ERedDotKey.MAIL, ERedDotKey.ROOT);
}
```

**중요**: 부모 노드를 먼저 생성한 후 자식 노드를 생성해야 합니다.

## API 레퍼런스

### 조건 관리
| 메서드 | 설명 |
|--------|------|
| `RegisterCondition(key, Func<bool>)` | 리프 노드에 조건 함수 등록 |
| `UnregisterCondition(key)` | 조건 함수 해제 |

### Dirty 관리
| 메서드 | 설명 |
|--------|------|
| `MarkDirty(key)` | 특정 노드 + 조상 체인을 dirty 마킹 |
| `MarkAllDirty()` | 전체 노드 dirty 마킹 |

### UI 바인딩
| 메서드 | 설명 |
|--------|------|
| `AddListener(key, Action<bool>)` | 리스너 등록 (등록 즉시 현재 상태 알림) |
| `RemoveListener(key, Action<bool>)` | 리스너 해제 |

### 상태 조회
| 메서드 | 설명 |
|--------|------|
| `IsActive(key)` | 특정 노드의 활성 상태 반환 |

## Dirty Flag 처리 흐름

```
1. 외부 시스템이 MarkDirty(key) 호출
2. 해당 노드부터 루트까지 조상 체인 전체를 dirtyNodes에 추가
   - 이미 dirty인 노드를 만나면 상위 체인도 이미 dirty이므로 중단
3. LateUpdate 도달
4. dirtyNodes를 깊이 내림차순 정렬 (리프 우선)
5. 정렬 순서대로 Evaluate() 호출
   - 리프: condition() 평가
   - 브랜치: 자식 isActive 집계
6. 상태 변경된 노드만 리스너에 알림
```

## 사용 예시

### 조건 등록 (외부 시스템)
```csharp
// MailManager에서 조건 등록
void Initialize()
{
    RedDotConditionManager.Instance.RegisterCondition(
        ERedDotKey.MAIL,
        () => unreadMailCount > 0
    );
}

// 메일 수신 시 Dirty 마킹
void OnMailReceived(Mail mail)
{
    unreadMailCount++;
    RedDotConditionManager.Instance.MarkDirty(ERedDotKey.MAIL);
}

// 메일 읽음 처리 시 Dirty 마킹
void OnMailRead(Mail mail)
{
    unreadMailCount--;
    RedDotConditionManager.Instance.MarkDirty(ERedDotKey.MAIL);
}
```

### 전체 갱신 (로그인 직후 등)
```csharp
RedDotConditionManager.Instance.MarkAllDirty();
```

### 코드에서 직접 상태 확인
```csharp
if (RedDotConditionManager.Instance.IsActive(ERedDotKey.MAIL))
{
    // 읽지 않은 메일이 있음
}
```

## 주의사항

- `InitTree()`에서 부모 노드를 반드시 자식보다 먼저 생성해야 합니다
- `MarkDirty()`는 즉시 평가하지 않고 `LateUpdate`에서 일괄 처리합니다
- 한 프레임에 같은 노드를 여러 번 `MarkDirty()`해도 한 번만 평가합니다
- `DontDestroyOnLoad` 싱글톤이므로 씬 전환 후에도 트리 상태가 유지됩니다
