# GameDataLoader

## 개요
테이블 SO들을 관리하고 접근할 수 있는 싱글톤 클래스입니다.
**버전 + 델타 방식**으로 효율적인 서버 데이터 동기화를 지원합니다.

## 클래스 정보
- **위치**: `Assets/Script/Common/GameData/GameDataLoader.cs`
- **상속**: `Singleton<GameDataLoader>`
- **특성**: `[SingletonFlag(ESingletonFlag.DONT_DESTROY)]`

## 동기화 방식

### 버전 + 델타 방식
```
1. 클라이언트 → 서버: "내 ItemTable 버전은 1.0.2야"
2. 서버 → 클라이언트:
   - 버전 같음 → "최신이야" (NO_CHANGE)
   - 차이 작음 → "변경된 행만 보낼게" (DELTA)
   - 차이 큼 → "전체 데이터 보낼게" (FULL)
```

### 동기화 타입 (ESyncType)
| 타입 | 설명 |
|------|------|
| `NO_CHANGE` | 버전 동일, 업데이트 불필요 |
| `DELTA` | 추가/수정/삭제된 행만 반영 |
| `FULL` | 전체 데이터 교체 |

## 데이터 구조

### TableSyncData (서버 → 클라이언트)
```csharp
[Serializable]
public class TableSyncData
{
    public string Version;        // 서버 테이블 버전
    public bool SupportsDelta;    // 델타 지원 여부
    public string FullDataCsv;    // 전체 데이터 (FULL 시)
    public DeltaData DeltaData;   // 델타 데이터 (DELTA 시)
}
```

### DeltaData
```csharp
[Serializable]
public class DeltaData
{
    public string AddedRowsCsv;    // 추가된 행들 (CSV)
    public string ModifiedRowsCsv; // 수정된 행들 (CSV)
    public List<int> DeletedIds;   // 삭제된 행 ID 목록
}
```

### SyncResult (동기화 결과)
```csharp
[Serializable]
public class SyncResult
{
    public bool Success;
    public string TableName;
    public ESyncType SyncType;
    public string PreviousVersion;
    public string NewVersion;
    public int AddedCount;
    public int ModifiedCount;
    public int DeletedCount;
    public string Message;
}
```

## 사용 예시

### 기본 사용
```csharp
// 테이블 가져오기
var itemTable = GameDataLoader.Instance.GetTable<ItemTable>();

// ID로 행 조회
var item = itemTable.GetRowById(5);

// 버전 확인
var version = GameDataLoader.Instance.GetTableVersion("ItemTable");
```

### 서버 동기화 (델타)
```csharp
// 서버에서 동기화 데이터 수신
var syncData = new TableSyncData
{
    Version = "1.0.5",
    SupportsDelta = true,
    DeltaData = new DeltaData
    {
        AddedRowsCsv = "id,name,price\nint,string,int\n10,NewItem,500",
        ModifiedRowsCsv = "id,name,price\nint,string,int\n5,UpdatedItem,300",
        DeletedIds = new List<int> { 3, 7 }
    }
};

// 동기화 실행
var result = GameDataLoader.Instance.SyncOnServerData("ItemTable", syncData);

if (result.Success)
{
    Debug.Log($"동기화 완료: {result.SyncType}");
    Debug.Log($"추가: {result.AddedCount}, 수정: {result.ModifiedCount}, 삭제: {result.DeletedCount}");
}
```

### 서버 동기화 (전체 교체)
```csharp
var syncData = new TableSyncData
{
    Version = "2.0.0",
    SupportsDelta = false,
    FullDataCsv = "id,name,price\nint,string,int\n1,Sword,100\n2,Shield,80"
};

GameDataLoader.Instance.SyncOnServerData("ItemTable", syncData);
```

### 여러 테이블 일괄 동기화
```csharp
var syncDataMap = new Dictionary<string, TableSyncData>
{
    { "ItemTable", itemSyncData },
    { "MonsterTable", monsterSyncData }
};

var results = GameDataLoader.Instance.SyncOnServerData(syncDataMap);

foreach (var kvp in results)
{
    Debug.Log($"{kvp.Key}: {kvp.Value.Message}");
}
```

### 동기화 이벤트 구독
```csharp
GameDataLoader.Instance.OnTableSynced += (tableName, result) =>
{
    Debug.Log($"{tableName} 동기화됨: {result.SyncType}");
};
```

## BaseTable 변경 사항

### Row 클래스
```csharp
// 모든 Row는 id 필드 필수 (델타 업데이트용)
public abstract class Row
{
    public int id;
}
```

### 버전 정보
```csharp
[Header("버전 정보")]
[SerializeField]
protected string version = "1.0.0";

public string Version => version;
```

### 주요 메서드
| 메서드 | 설명 |
|--------|------|
| `GetRowById(int id)` | ID로 행 조회 |
| `HotFix(rows, version)` | 전체 데이터 교체 |
| `HotFixDelta(added, modified, deleted, version)` | 델타 업데이트 적용 |
| `HotFixRowById(id, row)` | 특정 ID 행 교체 |
| `HotFixRemoveById(id)` | 특정 ID 행 삭제 |

## API 레퍼런스

### GameDataLoader 메서드

| 메서드 | 설명 |
|--------|------|
| `GetTable<T>()` | 타입으로 테이블 가져오기 |
| `GetTableByName(string)` | 이름으로 테이블 가져오기 |
| `GetTableVersion(string)` | 테이블 버전 조회 |
| `GetAllTableVersions()` | 모든 테이블 버전 조회 |
| `SyncOnServerData(string, TableSyncData)` | 단일 테이블 동기화 |
| `SyncOnServerData(Dictionary)` | 다중 테이블 동기화 |

### 이벤트

| 이벤트 | 설명 |
|--------|------|
| `OnInitialized` | 테이블 로드 완료 |
| `OnTableSynced` | 테이블 동기화 완료 (tableName, SyncResult) |

## 서버 구현 가이드

서버에서 델타 데이터를 생성하는 예시:

```csharp
// 서버 측 델타 생성 로직
public TableSyncData CreateSyncData(string tableName, string clientVersion)
{
    var serverVersion = GetCurrentVersion(tableName);

    // 버전 동일
    if (clientVersion == serverVersion)
    {
        return new TableSyncData { Version = serverVersion, SupportsDelta = false };
    }

    // 버전 차이가 크면 전체 교체
    if (IsVersionGapTooLarge(clientVersion, serverVersion))
    {
        return new TableSyncData
        {
            Version = serverVersion,
            SupportsDelta = false,
            FullDataCsv = GetFullTableCsv(tableName)
        };
    }

    // 델타 업데이트
    return new TableSyncData
    {
        Version = serverVersion,
        SupportsDelta = true,
        DeltaData = new DeltaData
        {
            AddedRowsCsv = GetAddedRowsCsv(tableName, clientVersion),
            ModifiedRowsCsv = GetModifiedRowsCsv(tableName, clientVersion),
            DeletedIds = GetDeletedIds(tableName, clientVersion)
        }
    };
}
```

## 주의사항

- 모든 테이블 Row는 `id` 필드 필수 (델타 업데이트에서 행 식별용)
- 테이블 SO는 `Resources/Datas/` 폴더에 위치
- 델타 업데이트 시 삭제 → 수정 → 추가 순서로 처리
- `OnTableSynced` 이벤트 구독 후 구독 해제 권장
