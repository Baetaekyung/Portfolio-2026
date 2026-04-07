# UnityDictionary

## 개요
Unity Inspector에서 **직렬화 가능한 제네릭 딕셔너리**입니다.
기본 `Dictionary<TKey, TValue>`는 Unity에서 직렬화되지 않는 문제를 해결합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/Utils/UnityDictionary.cs`
- **제네릭**: `UnityDictionary<TKey, TValue>`

## 핵심 기능

### 직렬화 지원
- `ISerializationCallbackReceiver` 인터페이스 구현
- Inspector에서 키-밸류 쌍을 가로로 표시
- 씬/프리팹 저장 시 데이터 유지

### Dictionary 호환
- 표준 `Dictionary<TKey, TValue>`와 동일한 API 제공
- 인덱서, LINQ 등 기존 Dictionary 사용법 그대로 적용 가능

## 데이터 구조

### SerializableKeyValuePair
```csharp
[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;      // 키
    public TValue value;  // 밸류
}
```

### UnityDictionary
```csharp
[Serializable]
public class UnityDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    private List<SerializableKeyValuePair<TKey, TValue>> _serializedList;  // 직렬화용 리스트
    private Dictionary<TKey, TValue> _dictionary;                           // 런타임 딕셔너리
    private bool _isDirty;                                                  // 코드 변경 시 동기화 플래그
}
```

## 사용 예시

### 필드 선언
```csharp
// string -> int 딕셔너리
[SerializeField]
private UnityDictionary<string, int> itemCounts;

// GameObject -> float 딕셔너리
[SerializeField]
private UnityDictionary<GameObject, float> objectScales;
```

### 런타임 사용
```csharp
// 값 추가
itemCounts["sword"] = 5;
itemCounts.Add("shield", 3);

// 값 조회
int count = itemCounts["sword"];
if (itemCounts.TryGetValue("potion", out int value))
{
    Debug.Log($"Potion count: {value}");
}

// 순회
foreach (var pair in itemCounts)
{
    Debug.Log($"{pair.Key}: {pair.Value}");
}

// 삭제
itemCounts.Remove("sword");
itemCounts.Clear();
```

## API 레퍼런스

### 주요 메서드
| 메서드 | 설명 |
|--------|------|
| `Add(TKey, TValue)` | 키-밸류 쌍 추가 |
| `Remove(TKey)` | 키로 항목 삭제 |
| `TryGetValue(TKey, out TValue)` | 키로 값 조회 시도 |
| `ContainsKey(TKey)` | 키 존재 여부 확인 |
| `ContainsValue(TValue)` | 값 존재 여부 확인 |
| `Clear()` | 모든 항목 삭제 |

### 프로퍼티
| 프로퍼티 | 설명 |
|----------|------|
| `Count` | 항목 개수 |
| `Keys` | 모든 키 컬렉션 |
| `Values` | 모든 값 컬렉션 |
| `this[TKey]` | 인덱서로 값 접근 |

### ISerializationCallbackReceiver
| 메서드 | 설명 |
|--------|------|
| `OnBeforeSerialize()` | 직렬화 전: 코드에서 변경된 경우에만 Dictionary -> List 동기화 |
| `OnAfterDeserialize()` | 역직렬화 후: List -> Dictionary 변환 |

## Inspector 표시

PropertyDrawer를 통해 Inspector에서 다음과 같이 표시됩니다:
```
▼ Item Counts
   [0] | Key: sword    | Value: 5  |
   [1] | Key: shield   | Value: 3  |
   [+] Add Element
```

## 지원 타입

### 키(TKey)로 사용 가능한 타입
- 기본 타입: `int`, `float`, `string`, `bool` 등
- Unity 타입: `Vector2`, `Vector3`, `Color` 등
- Enum 타입
- Unity 오브젝트 참조: `GameObject`, `ScriptableObject` 등

### 밸류(TValue)로 사용 가능한 타입
- 키와 동일한 모든 타입
- `[Serializable]` 속성이 있는 커스텀 클래스/구조체

## 주의사항

- 키는 null이 될 수 없음
- 중복 키 추가 시 예외 발생 (`Add`) 또는 덮어쓰기 (`인덱서`)
- 런타임에서 Dictionary 수정 후 Inspector 반영은 다음 직렬화 시점에 적용
- 커스텀 클래스를 키로 사용할 경우 `GetHashCode()`와 `Equals()` 구현 필요
