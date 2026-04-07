# TableParser 구현 목표

## 개요
GoogleSpreadSheet에서 데이터를 자동으로 파싱하여 BaseTable을 상속받은 테이블 클래스와 데이터 클래스를 생성하는 Unity 에디터 툴입니다.

**웹에 게시된 스프레드시트에서 CSV 형식으로 데이터를 가져오므로 API 키가 필요하지 않습니다.**

## 구현 요구사항

### 1. 기본 구조
- Unity Editor 전용 클래스 (`Editor` 폴더에 위치)
- `EditorWindow`를 상속받아 GUI 제공
- 웹에 게시된 GoogleSpreadSheet에서 CSV 형식으로 데이터 가져오기
- 자동 코드 생성 기능
- `SpreadSheetConfig` ScriptableObject를 통한 설정 관리

### 2. SpreadSheetConfig (설정 관리 클래스)

#### 클래스 구조
```csharp
[CreateAssetMenu(fileName = "SpreadSheetConfig", menuName = "Data/SpreadSheetConfig")]
public class SpreadSheetConfig : ScriptableObject
{
    [Header("GoogleSpreadSheet 설정")]
    [SerializeField]
    private string spreadsheetId;

    [Header("시트 목록")]
    [SerializeField]
    private List<string> sheetNames = new List<string>();

    [Header("출력 경로 설정")]
    [SerializeField]
    private string tableOutputPath = "Assets/Script/Data/Table";

    [SerializeField]
    private string dataOutputPath = "Assets/Script/Data/TableData";

    [SerializeField]
    private string assetOutputPath = "Assets/Resources/Tables";

    // 프로퍼티
    public string SpreadsheetId => spreadsheetId;
    public IReadOnlyList<string> SheetNames => sheetNames;
    public string TableOutputPath => tableOutputPath;
    public string DataOutputPath => dataOutputPath;
    public string AssetOutputPath => assetOutputPath;

    // CSV 다운로드 URL 형식 (웹에 게시된 스프레드시트용)
    public const string BASE_URL = "https://docs.google.com/spreadsheets/d/";

    // 설정 유효성 검사
    public bool IsValid => !string.IsNullOrEmpty(spreadsheetId);

    // 시트별 CSV URL 생성
    public string GetSheetUrl(string sheetName)
    {
        return $"{BASE_URL}{spreadsheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
    }
}
```

#### 설정 항목
| 항목 | 설명 | 기본값 |
|------|------|--------|
| spreadsheetId | GoogleSpreadSheet의 고유 ID | - |
| sheetNames | 파싱할 시트 이름 목록 | 빈 리스트 |
| tableOutputPath | 생성될 Table 클래스 경로 | Assets/Script/Data/Table |
| dataOutputPath | 생성될 Data 클래스 경로 | Assets/Script/Data/TableData |
| assetOutputPath | 생성될 에셋 파일 경로 | Assets/Resources/Tables |

#### Spreadsheet ID 찾는 방법
Google Sheets URL에서 ID를 추출합니다:
```
https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit#gid=0
                                        ^^^^^^^^^^^^^^^^
                                        이 부분이 Spreadsheet ID
```

### 3. GoogleSpreadSheet 웹 게시 설정

**중요: 스프레드시트를 웹에 게시해야 데이터를 가져올 수 있습니다.**

1. Google Sheets에서 스프레드시트 열기
2. `파일` > `공유` > `웹에 게시` 클릭
3. "전체 문서" 또는 원하는 시트 선택
4. `게시` 버튼 클릭

#### CSV URL 형식
```
https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/gviz/tq?tqx=out:csv&sheet={SHEET_NAME}
```

### 4. 데이터 파싱

#### 시트 구조
```
첫 번째 행: 컬럼 이름 (필드 이름)
두 번째 행: 데이터 타입 (int, string, float, bool 등)
세 번째 행부터: 실제 데이터
```

예시:
```
| id    | itemName | price | isRare |
|-------|----------|-------|--------|
| int   | string   | int   | bool   |
| 1     | Sword    | 100   | false  |
| 2     | Shield   | 80    | false  |
| 3     | Potion   | 50    | true   |
```

#### CSV 파싱 로직
```csharp
// CSV 데이터 가져오기
private string GetCsvData(string sheetName)
{
    var url = _config.GetSheetUrl(sheetName);
    var webClient = new WebClient();
    webClient.Encoding = Encoding.UTF8;
    return webClient.DownloadString(url);
}

// CSV 파싱 (따옴표, 줄바꿈 처리 포함)
private List<List<string>> ParseCsv(string csvData)
{
    // 줄바꿈이 셀 내에 있을 수 있으므로 특별한 처리 필요
    // 따옴표로 감싸진 셀 내의 쉼표는 구분자가 아님
}
```

### 5. 코드 자동 생성

#### (시트이름)Table.cs 생성
```csharp
private void GenerateTableClass(SheetData sheetData)
{
    var className = sheetData.sheetName + "Table";
    var path = $"{_config.TableOutputPath}/{className}.cs";

    // BaseTable<T>를 상속받는 클래스 생성
    // 내부에 RowData 클래스 정의
}
```

#### (시트이름).cs 생성
```csharp
private void GenerateDataClass(SheetData sheetData)
{
    var className = sheetData.sheetName;
    var tableName = className + "Table";
    var path = $"{_config.DataOutputPath}/{className}.cs";

    // TableName.RowData를 상속받는 클래스 생성
}
```

#### 타입 변환
```csharp
private string ConvertToCSType(string sheetType)
{
    switch (sheetType.ToLower())
    {
        case "int":
        case "integer":
            return "int";
        case "float":
        case "double":
            return "float";
        case "string":
        case "text":
            return "string";
        case "bool":
        case "boolean":
            return "bool";
        case "long":
            return "long";
        default:
            return "string"; // 기본값
    }
}
```

### 6. EditorWindow GUI

```csharp
public class TableParser : EditorWindow
{
    private SpreadSheetConfig _config;
    private Vector2 _scrollPosition;
    private Dictionary<string, bool> _selectedSheets = new Dictionary<string, bool>();

    [MenuItem("Tools/Table Parser")]
    public static void ShowWindow()
    {
        GetWindow<TableParser>("Table Parser");
    }

    private void OnGUI()
    {
        // 1. SpreadSheetConfig 에셋 참조 필드
        // 2. 설정 유효성 검사 (ID가 있는지, 시트 목록이 있는지)
        // 3. 시트 목록 체크박스 표시
        // 4. "선택한 시트 파싱" / "모든 시트 파싱" 버튼
    }
}
```

## 사용 순서

1. **SpreadSheetConfig 생성**
   - Unity 메뉴: `Assets > Create > Data > SpreadSheetConfig`
   - 또는 TableParser 창에서 자동 생성

2. **GoogleSpreadSheet 웹에 게시**
   - Google Sheets에서 `파일 > 공유 > 웹에 게시` 클릭
   - 전체 문서 또는 원하는 시트 선택 후 게시

3. **SpreadSheetConfig 설정**
   - Inspector에서 Spreadsheet ID 입력 (URL에서 추출)
   - 시트 이름 목록 추가 (Sheet Names)
   - 필요시 출력 경로 변경

4. **TableParser 실행**
   - Unity 메뉴: `Tools > Table Parser`
   - SpreadSheetConfig 에셋 선택
   - 파싱할 시트 선택

5. **시트 파싱**
   - "선택한 시트 파싱" 또는 "모든 시트 파싱" 클릭
   - 클래스 파일 자동 생성

6. **컴파일 후 에셋 생성**
   - Unity가 컴파일을 완료한 후
   - 다시 파싱하여 ScriptableObject 에셋 생성

7. **코드에서 사용**
   ```csharp
   var itemTable = Resources.Load<ItemTable>("Tables/ItemTable");
   foreach (var item in itemTable.Rows)
   {
       Debug.Log(item.itemName);
   }
   ```

## 파일 구조

```
Project_Q/
├── Project_Q_Unity/
│   └── Assets/
│       ├── Resources/
│       │   ├── SpreadSheetConfig.asset
│       │   └── Tables/
│       │       ├── ItemTable.asset
│       │       └── MonsterTable.asset
│       └── Script/
│           ├── Data/
│           │   ├── Table/
│           │   │   ├── BaseTable.cs
│           │   │   ├── SpreadSheetConfig.cs
│           │   │   ├── ItemTable.cs (자동 생성)
│           │   │   └── MonsterTable.cs (자동 생성)
│           │   └── TableData/
│           │       ├── Item.cs (자동 생성)
│           │       └── Monster.cs (자동 생성)
│           └── Editor/
│               └── TableParser.cs
│
└── Project_Q_Server/
    ├── Datas/
    │   ├── Item.csv (자동 생성 - 유니티 파싱 시 복제)
    │   └── Monster.csv (자동 생성 - 유니티 파싱 시 복제)
    └── Data/
        └── Table/
            ├── BaseTable.cs (순수 C# 클래스)
            ├── CsvParser.cs (CSV 파싱 유틸리티)
            ├── ItemTable.cs (자동 생성)
            └── MonsterTable.cs (자동 생성)
```

## 서버 연동

### 개요
유니티에서 파싱을 실행하면 자동으로 서버 프로젝트에도 데이터가 복제됩니다:
- CSV 파일이 `Project_Q_Server/Datas/`에 저장됩니다.
- 서버용 Table 클래스가 `Project_Q_Server/Data/Table/`에 생성됩니다.

### 서버용 BaseTable
서버는 ScriptableObject를 사용할 수 없으므로 순수 C# 클래스로 구현됩니다:

```csharp
namespace ProjectQ.Data
{
    public abstract class BaseTable<TRow>
        where TRow : BaseTable<TRow>.Row, new()
    {
        public abstract class Row { }

        protected List<TRow> _rows = new List<TRow>();
        public IReadOnlyList<TRow> Rows => _rows;

        // CSV 파일에서 데이터 로드
        public void LoadFromCsv(string csvPath);
    }
}
```

### 서버에서 사용 예제
```csharp
using ProjectQ.Data;

// 테이블 인스턴스 생성
var itemTable = new ItemTable();

// CSV 파일에서 데이터 로드
itemTable.LoadFromCsv("Datas/Item.csv");

// 데이터 접근
foreach (var item in itemTable.Rows)
{
    Console.WriteLine(item.itemName);
}
```

### SpreadSheetConfig 서버 설정
| 항목 | 설명 | 기본값 |
|------|------|--------|
| serverCsvOutputPath | 서버용 CSV 파일 저장 경로 | ../Project_Q_Server/Datas |
| serverTableOutputPath | 서버용 Table 클래스 생성 경로 | ../Project_Q_Server/Data/Table |

## 주의사항

- **웹 게시 필수**: 스프레드시트를 반드시 웹에 게시해야 합니다 (공개 읽기 가능)
- **시트 이름 일치**: Config의 시트 이름이 실제 스프레드시트의 시트 이름과 정확히 일치해야 합니다
- **네트워크 오류**: 인터넷 연결 필요, 오프라인에서는 동작하지 않음
- **타입 검증**: 시트의 데이터 타입과 실제 데이터가 일치하는지 검증
- **코드 재생성**: 기존 클래스 파일을 덮어쓰므로 수동 수정 사항은 별도 클래스로 분리
- **특수문자 처리**: CSV에서 쉼표, 줄바꿈, 따옴표가 포함된 데이터는 자동 처리됨

## API 키 없이 동작하는 이유

Google Sheets의 "웹에 게시" 기능을 사용하면:
- 공개 URL로 CSV 데이터에 접근 가능
- API 키나 OAuth 인증 불필요
- 간단한 HTTP GET 요청으로 데이터 획득

URL 형식:
```
https://docs.google.com/spreadsheets/d/{ID}/gviz/tq?tqx=out:csv&sheet={SHEET_NAME}
```

## 확장 기능

### 1. 로컬 캐싱
- 파싱한 데이터를 로컬에 캐싱
- 오프라인 작업 지원

### 2. 다중 언어 지원
- 시트에 언어별 컬럼 추가
- 지역화 데이터 자동 생성

### 3. 유효성 검증
- 필수 필드 체크
- 값 범위 검증
- 중복 데이터 확인

### 4. 버전 관리
- 데이터 버전 관리
- 변경 이력 추적
