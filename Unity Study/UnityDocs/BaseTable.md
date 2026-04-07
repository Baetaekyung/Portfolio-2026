# BaseTable 구현 목표

## 개요
GoogleSpreadSheet에서 불러온 데이터를 저장하는 ScriptableObject 기반 테이블 클래스를 구현합니다.

## 구현 요구사항

### 1. 기본 구조
- `ScriptableObject`를 상속받아야 합니다
- 제네릭 형식으로 구현되어야 합니다: `BaseTable<TRow> where TRow : BaseTable<TRow>.Row`
- `abstract` 클래스로 선언하여 직접 인스턴스화를 방지합니다

### 2. 핵심 멤버

#### Row 내부 클래스
```csharp
[Serializable]
public abstract class Row
{
    // 모든 테이블 Row의 공통 필드
}
```
- `abstract` 클래스로 선언
- `[Serializable]` 속성 필수 (Unity Serialization을 위해)
- 모든 테이블 행의 기본 클래스 역할
- 공통으로 사용되는 필드 정의 가능 (예: ID, Name 등)

#### rows 필드
```csharp
[SerializeField]
protected List<TRow> rows = new List<TRow>();
```
- 테이블의 모든 행 데이터를 저장
- `List<TRow>` 타입으로 제네릭 Row 타입 저장
- `[SerializeField]`로 직렬화하여 Unity에서 저장 가능
- `protected`로 선언하여 상속받은 클래스에서 접근 가능

#### Rows 프로퍼티
```csharp
public IReadOnlyList<TRow> Rows => rows;
```
- 외부에서 읽기 전용으로 행 데이터에 접근
- `IReadOnlyList<TRow>`로 반환하여 수정 방지
- 데이터 무결성 유지

### 3. 데이터 접근 메서드

#### GetRow 메서드
```csharp
public TRow GetRow(int index)
{
    if (index < 0 || index >= rows.Count)
        return null;

    return rows[index];
}
```
- 인덱스로 특정 행 데이터 반환
- 범위 체크를 통한 안전한 접근

#### Count 프로퍼티
```csharp
public int Count => rows.Count;
```
- 테이블의 총 행 개수 반환

### 4. 데이터 관리 메서드 (Editor 전용)

#### SetRows 메서드
```csharp
#if UNITY_EDITOR
public void SetRows(List<TRow> newRows)
{
    rows = newRows;
    UnityEditor.EditorUtility.SetDirty(this);
}
#endif
```
- TableParser에서 파싱한 데이터를 설정
- `UNITY_EDITOR` 조건부 컴파일로 에디터에서만 사용 가능
- `SetDirty`로 변경사항을 Unity에 알림

#### ClearRows 메서드
```csharp
#if UNITY_EDITOR
public void ClearRows()
{
    rows.Clear();
    UnityEditor.EditorUtility.SetDirty(this);
}
#endif
```
- 모든 행 데이터 삭제
- 재파싱 전 데이터 초기화에 사용

### 5. 제네릭 구조

```csharp
public abstract class BaseTable<TRow> : ScriptableObject
    where TRow : BaseTable<TRow>.Row
```

#### 제약 조건
- `TRow : BaseTable<TRow>.Row`: TRow는 반드시 이 테이블의 Row 클래스를 상속받아야 함
- 타입 안정성 보장
- 각 테이블은 자신만의 Row 타입을 가짐

## 자동 생성되는 클래스 구조

### (시트이름)Table.cs 예시
```csharp
// ItemTable.cs (자동 생성)
public class ItemTable : BaseTable<ItemTable.RowData>
{
    [Serializable]
    public class RowData : Row
    {
        // GoogleSpreadSheet의 컬럼에 맞는 필드들
        public int id;
        public string itemName;
        public int price;
        public string description;
    }
}
```

### (시트이름).cs 예시
```csharp
// Item.cs (자동 생성)
[Serializable]
public class Item : ItemTable.RowData
{
    // 추가 로직이나 계산된 프로퍼티
    public bool IsExpensive => price > 1000;

    public string GetDisplayName()
    {
        return $"[{id}] {itemName}";
    }
}
```

## 사용 예시

### 테이블 생성 및 사용
```csharp
// 1. TableParser로 GoogleSpreadSheet에서 데이터 파싱
// -> ItemTable.asset 생성됨 (ScriptableObject)

// 2. 런타임에서 사용
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ItemTable itemTable;

    void Start()
    {
        // 모든 아이템 순회
        foreach (var item in itemTable.Rows)
        {
            Debug.Log($"Item: {item.itemName}, Price: {item.price}");
        }

        // 특정 아이템 접근
        var firstItem = itemTable.GetRow(0);
        if (firstItem != null)
        {
            Debug.Log(firstItem.itemName);
        }

        // 총 아이템 개수
        Debug.Log($"Total Items: {itemTable.Count}");
    }
}
```

### LINQ를 사용한 데이터 조회
```csharp
using System.Linq;

// 가격으로 아이템 찾기
var expensiveItems = itemTable.Rows.Where(item => item.price > 1000);

// ID로 아이템 찾기
var item = itemTable.Rows.FirstOrDefault(i => i.id == 101);

// 이름으로 정렬
var sortedItems = itemTable.Rows.OrderBy(item => item.itemName);
```

## 파일 구조

```
Assets/
├── Script/
│   ├── Data/
│   │   ├── Table/
│   │   │   ├── BaseTable.cs
│   │   │   ├── ItemTable.cs (자동 생성)
│   │   │   ├── MonsterTable.cs (자동 생성)
│   │   │   └── ...
│   │   └── TableData/
│   │       ├── Item.cs (자동 생성)
│   │       ├── Monster.cs (자동 생성)
│   │       └── ...
│   └── Editor/
│       └── TableParser.cs
└── Resources/
    └── Tables/
        ├── ItemTable.asset
        ├── MonsterTable.asset
        └── ...
```

## 주의사항

- **ScriptableObject 특성**: 에셋 파일로 저장되므로 런타임에 데이터 수정이 불가능합니다 (읽기 전용)
- **Serialization 제약**: Unity가 직렬화할 수 있는 타입만 사용 가능 (int, string, bool, Vector3 등)
- **제네릭 제약**: ScriptableObject는 제네릭을 완전히 지원하지 않으므로, 실제 구현 시 주의 필요
- **대용량 데이터**: 너무 많은 데이터는 메모리 문제를 일으킬 수 있으므로 분할 고려
- **에디터 전용 메서드**: `SetRows`, `ClearRows`는 빌드에 포함되지 않음

## GoogleSpreadSheet 연동

### 스프레드시트 구조
```
| id | itemName | price | description |
|----|----------|-------|-------------|
| 1  | Sword    | 100   | Basic sword |
| 2  | Shield   | 80    | Basic shield|
| 3  | Potion   | 50    | HP recovery |
```

### 파싱 규칙
- 첫 번째 행: 컬럼 이름 (필드 이름으로 사용)
- 두 번째 행부터: 실제 데이터
- 컬럼 이름은 C# 변수 명명 규칙을 따라야 함
- 타입은 데이터로부터 자동 추론 또는 별도 타입 행 지정

## 확장 가능성

### 인덱싱 추가
```csharp
public abstract class BaseTable<TRow> : ScriptableObject
    where TRow : BaseTable<TRow>.Row
{
    private Dictionary<int, TRow> _idIndex;

    public TRow GetById(int id)
    {
        if (_idIndex == null)
            BuildIndex();

        return _idIndex.TryGetValue(id, out var row) ? row : null;
    }

    private void BuildIndex()
    {
        _idIndex = new Dictionary<int, TRow>();
        foreach (var row in rows)
        {
            // id 필드가 있다고 가정
            // _idIndex[row.id] = row;
        }
    }
}
```

### 유효성 검증
```csharp
#if UNITY_EDITOR
public virtual bool Validate()
{
    // 중복 ID 체크
    // 필수 필드 체크
    // 값 범위 체크 등
    return true;
}
#endif
```
