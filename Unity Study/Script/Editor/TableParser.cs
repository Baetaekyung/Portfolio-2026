using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/*
GoogleSpreadSheet에서 데이터를 자동으로 파싱하여
BaseTable을 상속받은 테이블 클래스와 데이터 클래스를 생성하는 에디터 툴
웹에 게시된 스프레드시트에서 CSV 형식으로 데이터를 가져옴
*/
public class TableParser : EditorWindow
{
    private SpreadSheetConfig _config;
    private Vector2 _scrollPosition;
    private Dictionary<string, bool> _selectedSheets = new Dictionary<string, bool>();

    // 컴파일 후 자동 SO 생성을 위한 대기 시트 목록 저장 키
    private const string PENDING_SHEETS_KEY = "TableParser_PendingSheets";
    private const string PENDING_CONFIG_PATH_KEY = "TableParser_PendingConfigPath";

    [MenuItem("Tools/Table Parser")]
    public static void ShowWindow()
    {
        GetWindow<TableParser>("Table Parser");
    }

    // 스크립트 리로드 후 대기 중인 SO 생성 처리
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // 대기 중인 시트가 있는지 확인
        var pendingSheets = EditorPrefs.GetString(PENDING_SHEETS_KEY, "");
        var configPath = EditorPrefs.GetString(PENDING_CONFIG_PATH_KEY, "");

        if (string.IsNullOrEmpty(pendingSheets) || string.IsNullOrEmpty(configPath))
            return;

        // 대기 중인 정보 클리어
        EditorPrefs.DeleteKey(PENDING_SHEETS_KEY);
        EditorPrefs.DeleteKey(PENDING_CONFIG_PATH_KEY);

        // Config 로드
        var config = AssetDatabase.LoadAssetAtPath<SpreadSheetConfig>(configPath);
        if (config == null)
        {
            Debug.LogError($"SpreadSheetConfig를 찾을 수 없습니다: {configPath}");
            return;
        }

        // 대기 중인 시트들의 SO 생성
        var sheetNames = pendingSheets.Split(',');
        Debug.Log($"컴파일 완료 - {sheetNames.Length}개 시트의 SO 생성 시작...");

        foreach (var sheetName in sheetNames)
        {
            if (string.IsNullOrEmpty(sheetName))
                continue;

            GenerateTableAssetStatic(sheetName, config);
        }

        AssetDatabase.Refresh();
        Debug.Log("모든 SO 생성 완료!");
    }

    private void OnGUI()
    {
        GUILayout.Label("GoogleSpreadSheet 설정", EditorStyles.boldLabel);

        // SpreadSheetConfig 에셋 참조
        _config = (SpreadSheetConfig)EditorGUILayout.ObjectField(
            "Config",
            _config,
            typeof(SpreadSheetConfig),
            false
        );

        // 설정 파일이 없으면 생성 버튼 표시
        if (_config == null)
        {
            EditorGUILayout.HelpBox("SpreadSheetConfig 에셋을 선택하거나 새로 생성하세요.", MessageType.Warning);

            if (GUILayout.Button("SpreadSheetConfig 생성"))
            {
                CreateConfigAsset();
            }

            return;
        }

        // 설정 유효성 검사
        if (!_config.IsValid)
        {
            EditorGUILayout.HelpBox("SpreadSheetConfig에서 Spreadsheet ID를 설정하세요.", MessageType.Warning);

            if (GUILayout.Button("Config 선택"))
            {
                Selection.activeObject = _config;
            }

            return;
        }

        // 시트 목록이 비어있으면 안내
        if (_config.SheetNames.Count == 0)
        {
            EditorGUILayout.HelpBox("SpreadSheetConfig에서 시트 이름을 추가하세요.", MessageType.Warning);

            if (GUILayout.Button("Config 선택"))
            {
                Selection.activeObject = _config;
            }

            return;
        }

        EditorGUILayout.Space();

        // 시트 목록 표시
        GUILayout.Label("시트 선택", EditorStyles.boldLabel);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (var sheetName in _config.SheetNames)
        {
            if (!_selectedSheets.ContainsKey(sheetName))
                _selectedSheets[sheetName] = false;

            _selectedSheets[sheetName] = EditorGUILayout.ToggleLeft(sheetName, _selectedSheets[sheetName]);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 파싱 버튼
        GUILayout.Label("1단계: 클래스 생성", EditorStyles.boldLabel);
        if (GUILayout.Button("선택한 시트 파싱"))
        {
            ParseSelectedSheets();
        }

        if (GUILayout.Button("모든 시트 파싱"))
        {
            ParseAllSheets();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("클래스 생성 후 컴파일이 완료되면 SO가 자동 생성됩니다.\n수동으로 생성하려면 아래 버튼을 사용하세요.", MessageType.Info);

        // SO 생성 버튼
        EditorGUILayout.Space();
        GUILayout.Label("2단계: SO 생성 및 데이터 로드", EditorStyles.boldLabel);

        if (GUILayout.Button("선택한 시트 SO 생성"))
        {
            GenerateSelectedSheetsAssets();
        }

        if (GUILayout.Button("모든 시트 SO 생성"))
        {
            GenerateAllSheetsAssets();
        }

        // 정리 버튼
        EditorGUILayout.Space();
        GUILayout.Label("3단계: 정리", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("시트 목록에 없는 테이블/데이터 스크립트와 SO를 삭제합니다.", MessageType.Warning);

        if (GUILayout.Button("사용하지 않는 스크립트 정리"))
        {
            CleanupUnusedScripts();
        }
    }

    // 선택한 시트들의 SO 생성
    private void GenerateSelectedSheetsAssets()
    {
        var generatedCount = 0;
        foreach (var kvp in _selectedSheets)
        {
            if (kvp.Value)
            {
                if (GenerateTableAssetFromCsv(kvp.Key))
                    generatedCount++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{generatedCount}개의 SO 생성이 완료되었습니다.", "확인");
    }

    // 모든 시트의 SO 생성
    private void GenerateAllSheetsAssets()
    {
        var generatedCount = 0;
        foreach (var sheetName in _config.SheetNames)
        {
            if (GenerateTableAssetFromCsv(sheetName))
                generatedCount++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{generatedCount}개의 SO 생성이 완료되었습니다.", "확인");
    }

    // CSV 파일에서 SO 생성 및 데이터 로드
    private bool GenerateTableAssetFromCsv(string sheetName)
    {
        try
        {
            // CSV 데이터 가져오기 (서버에 저장된 파일 또는 다시 다운로드)
            var csvData = GetCsvData(sheetName);
            if (string.IsNullOrEmpty(csvData))
                return false;

            // 시트 데이터 파싱
            var sheetData = ParseSheetFromCsv(sheetName, csvData);
            if (sheetData.columns.Count == 0)
            {
                Debug.LogError($"CSV 파싱 실패: {sheetName}");
                return false;
            }

            // SO 에셋 생성
            GenerateTableAsset(sheetData);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"{sheetName} SO 생성 중 오류: {e.Message}");
            return false;
        }
    }

    // CSV 문자열에서 시트 데이터 파싱
    private SheetData ParseSheetFromCsv(string sheetName, string csvData)
    {
        var data = ParseCsv(csvData);
        var sheetData = new SheetData { sheetName = sheetName };

        if (data.Count < 2)
        {
            Debug.LogError($"시트 데이터가 부족합니다 ({sheetName})");
            return sheetData;
        }

        var columnNames = data[0];
        var columnTypes = data[1];

        var hasExplicitTypeRow = columnTypes.Count > 0 &&
            columnTypes.Exists(t => IsValidExplicitType(t.Trim()));

        for (int i = 0; i < columnNames.Count; i++)
        {
            var columnName = columnNames[i].Trim();
            if (string.IsNullOrEmpty(columnName))
                continue;

            string name, type;
            if (hasExplicitTypeRow)
            {
                var columnType = i < columnTypes.Count ? columnTypes[i].Trim().ToLower() : "string";
                name = SanitizeIdentifier(columnName);
                type = IsValidExplicitType(columnType) ? columnType : "string";
            }
            else
            {
                var (extractedName, extractedType) = ExtractNameAndType(columnName);
                name = SanitizeIdentifier(extractedName);
                type = extractedType;
            }

            if (string.IsNullOrEmpty(name))
                continue;

            sheetData.columns.Add(new ColumnInfo
            {
                name = name,
                type = type,
                index = i
            });
        }

        var dataStartRow = hasExplicitTypeRow ? 2 : 1;
        for (int i = dataStartRow; i < data.Count; i++)
        {
            sheetData.rows.Add(data[i]);
        }

        return sheetData;
    }

    // SpreadSheetConfig 에셋 생성
    private void CreateConfigAsset()
    {
        var path = "Assets/Resources/SpreadSheetConfig.asset";

        // Resources 폴더 생성
        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");

        _config = ScriptableObject.CreateInstance<SpreadSheetConfig>();
        AssetDatabase.CreateAsset(_config, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = _config;
        Debug.Log($"SpreadSheetConfig 생성됨: {path}");
    }

    // CSV 데이터 가져오기
    private string GetCsvData(string sheetName)
    {
        try
        {
            var url = _config.GetSheetUrl(sheetName);
            Debug.Log($"CSV URL: {url}");

            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            var csvData = webClient.DownloadString(url);

            // 디버그: 처음 500자 출력
            Debug.Log($"CSV 데이터 (처음 500자):\n{csvData.Substring(0, Mathf.Min(500, csvData.Length))}");

            // 서버용 CSV 파일 저장
            SaveCsvToServer(sheetName, csvData);

            return csvData;
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 데이터 가져오기 실패 ({sheetName}): {e.Message}");
            return null;
        }
    }

    // 서버용 CSV 파일 저장
    private void SaveCsvToServer(string sheetName, string csvData)
    {
        try
        {
            var serverCsvPath = Path.GetFullPath(Path.Combine(Application.dataPath, _config.ServerCsvOutputPath));

            // 디렉토리 생성
            if (!Directory.Exists(serverCsvPath))
                Directory.CreateDirectory(serverCsvPath);

            var filePath = Path.Combine(serverCsvPath, $"{sheetName}.csv");
            File.WriteAllText(filePath, csvData, Encoding.UTF8);
            Debug.Log($"서버용 CSV 파일 저장: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"서버용 CSV 파일 저장 실패 ({sheetName}): {e.Message}");
        }
    }

    // CSV/TSV 파싱
    private List<List<string>> ParseCsv(string csvData)
    {
        var result = new List<List<string>>();
        if (string.IsNullOrEmpty(csvData)) return result;

        // 원본 데이터의 줄바꿈 문자 확인
        var hasRN = csvData.Contains("\r\n");
        var hasR = csvData.Contains("\r") && !hasRN;
        var hasN = csvData.Contains("\n");
        Debug.Log($"줄바꿈 문자: \\r\\n={hasRN}, \\r={hasR}, \\n={hasN}");

        // 줄바꿈 정규화 (\r\n, \r, \n 모두 \n으로 통일)
        csvData = csvData.Replace("\r\n", "\n").Replace("\r", "\n");

        // 끝의 공백/줄바꿈 제거 후 다시 줄바꿈 추가 (마지막 행 누락 방지)
        csvData = csvData.TrimEnd() + "\n";

        // 정규화 후 줄바꿈 개수 확인
        var lineBreakCount = csvData.Split('\n').Length - 1;
        Debug.Log($"정규화 후 줄바꿈 개수: {lineBreakCount}");

        // 구분자 자동 감지
        var delimiter = DetectDelimiter(csvData);

        var lines = ParseCsvLines(csvData);

        Debug.Log($"ParseCsvLines 결과: {lines.Count}개 라인");

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!string.IsNullOrWhiteSpace(line))
            {
                var cells = ParseCsvLine(line, delimiter);
                if (cells.Count > 0)
                {
                    result.Add(cells);

                    // 디버그: 처음 5행의 셀 내용 출력
                    if (i < 5)
                    {
                        Debug.Log($"행 {i}: [{string.Join("] | [", cells)}]");
                    }
                }
            }
        }

        Debug.Log($"최종 파싱된 행 수: {result.Count}");
        return result;
    }

    // CSV 라인 분리 (따옴표는 보존하여 ParseCsvLine에서 처리)
    private List<string> ParseCsvLines(string csvData)
    {
        var lines = new List<string>();
        var currentLine = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < csvData.Length; i++)
        {
            var c = csvData[i];

            if (c == '"')
            {
                // 따옴표를 그대로 보존 (ParseCsvLine에서 처리)
                currentLine.Append(c);

                if (inQuotes && i + 1 < csvData.Length && csvData[i + 1] == '"')
                {
                    // 이스케이프된 따옴표("")도 그대로 보존
                    currentLine.Append(csvData[i + 1]);
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == '\n' && !inQuotes)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
            else
            {
                currentLine.Append(c);
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        // 디버그: 줄바꿈 분리 결과
        Debug.Log($"줄바꿈 분리 결과: {lines.Count}개 라인, inQuotes 최종 상태: {inQuotes}");

        return lines;
    }

    // 한 줄을 셀로 분리 (CSV 쉼표 또는 TSV 탭 자동 감지)
    private List<string> ParseCsvLine(string line, char delimiter)
    {
        var cells = new List<string>();
        var currentCell = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // 이스케이프된 따옴표
                    currentCell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                cells.Add(currentCell.ToString());
                currentCell.Clear();
            }
            else
            {
                currentCell.Append(c);
            }
        }

        cells.Add(currentCell.ToString());
        return cells;
    }

    // 구분자 자동 감지 (탭이 있으면 TSV, 없으면 CSV)
    private char DetectDelimiter(string csvData)
    {
        // 첫 번째 줄에서 탭과 쉼표 개수 비교
        var firstLine = csvData.Split('\n')[0];
        var tabCount = firstLine.Split('\t').Length - 1;
        var commaCount = firstLine.Split(',').Length - 1;

        var delimiter = tabCount > commaCount ? '\t' : ',';
        Debug.Log($"감지된 구분자: {(delimiter == '\t' ? "탭(TSV)" : "쉼표(CSV)")}");

        return delimiter;
    }

    // 시트 데이터 파싱
    // 스프레드시트 형식: 1행 컬럼 이름, 2행 데이터 타입, 3행부터 데이터
    private SheetData ParseSheet(string sheetName)
    {
        var csvData = GetCsvData(sheetName);
        var data = ParseCsv(csvData);
        var sheetData = new SheetData { sheetName = sheetName };

        if (data.Count < 2)
        {
            Debug.LogError($"시트 데이터가 부족합니다 ({sheetName}): {data.Count}행 파싱됨, 최소 3행 필요 (1행: 이름, 2행: 타입, 3행~: 데이터)");
            return sheetData;
        }

        // 첫 번째 행: 컬럼 이름
        var columnNames = data[0];
        // 두 번째 행: 데이터 타입
        var columnTypes = data[1];

        // 두 번째 행이 유효한 타입 행인지 확인
        var hasExplicitTypeRow = columnTypes.Count > 0 &&
            columnTypes.Exists(t => IsValidExplicitType(t.Trim()));

        // ColumnInfo 생성
        for (int i = 0; i < columnNames.Count; i++)
        {
            var columnName = columnNames[i].Trim();

            // 빈 컬럼 이름은 건너뛰기
            if (string.IsNullOrEmpty(columnName))
                continue;

            string name, type;

            if (hasExplicitTypeRow)
            {
                // 두 번째 행에 유효한 타입이 있으면 기존 방식 사용
                var columnType = i < columnTypes.Count ? columnTypes[i].Trim().ToLower() : "string";
                name = SanitizeIdentifier(columnName);
                type = IsValidExplicitType(columnType) ? columnType : "string";
            }
            else
            {
                // 컬럼 이름에서 타입 suffix 추출 (예: "idint" -> "id", "int")
                var (extractedName, extractedType) = ExtractNameAndType(columnName);
                name = SanitizeIdentifier(extractedName);
                type = extractedType;
            }

            if (string.IsNullOrEmpty(name))
                continue;

            sheetData.columns.Add(new ColumnInfo
            {
                name = name,
                type = type,
                index = i
            });

            Debug.Log($"컬럼 파싱: 이름={name}, 타입={type}");
        }

        // 데이터 시작 행 결정 (타입 행이 있으면 3행부터, 없으면 2행부터)
        var dataStartRow = hasExplicitTypeRow ? 2 : 1;

        for (int i = dataStartRow; i < data.Count; i++)
        {
            sheetData.rows.Add(data[i]);
        }

        Debug.Log($"타입 행 존재: {hasExplicitTypeRow}, 데이터 시작 행: {dataStartRow + 1}");

        return sheetData;
    }

    // 문자열을 유효한 C# 식별자로 변환
    private string SanitizeIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var sb = new StringBuilder();

        foreach (var c in name)
        {
            // 영문자, 숫자, 언더스코어만 허용
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString();

        // 숫자로 시작하면 언더스코어 추가
        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        return result;
    }

    // 컬럼 이름에서 타입 suffix 추출 (예: "idint" -> ("id", "int"))
    private (string name, string type) ExtractNameAndType(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return (columnName, "string");

        // 지원하는 타입 suffix (긴 것부터 확인해야 함)
        var typeSuffixes = new[]
        {
            ("string", "string"),
            ("float", "float"),
            ("double", "float"),
            ("boolean", "bool"),
            ("bool", "bool"),
            ("integer", "int"),
            ("long", "long"),
            ("int", "int")
        };

        var lowerName = columnName.ToLower();

        foreach (var (suffix, csType) in typeSuffixes)
        {
            if (lowerName.EndsWith(suffix))
            {
                var name = columnName.Substring(0, columnName.Length - suffix.Length);
                if (!string.IsNullOrEmpty(name))
                {
                    return (name, csType);
                }
            }
        }

        // 타입 suffix가 없으면 기본값 string
        return (columnName, "string");
    }

    // 유효한 타입인지 확인
    private bool IsValidExplicitType(string type)
    {
        if (string.IsNullOrEmpty(type))
            return false;

        var validTypes = new[] { "int", "integer", "string", "text", "float", "double", "bool", "boolean", "long" };
        return Array.Exists(validTypes, t => t == type.ToLower());
    }

    // Table 클래스 생성
    private void GenerateTableClass(SheetData sheetData)
    {
        var className = sheetData.sheetName + "Table";
        var path = $"{_config.TableOutputPath}/{className}.cs";

        // 디렉토리 생성
        if (!Directory.Exists(_config.TableOutputPath))
            Directory.CreateDirectory(_config.TableOutputPath);

        var sb = new StringBuilder();

        // using 문
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();

        // 클래스 선언
        sb.AppendLine($"public class {className} : BaseTable<{className}.RowData>");
        sb.AppendLine("{");

        // Row 클래스
        sb.AppendLine("    [Serializable]");
        sb.AppendLine("    public class RowData : Row");
        sb.AppendLine("    {");

        // 필드 생성 (id는 부모 클래스 Row에 이미 존재하므로 제외)
        foreach (var column in sheetData.columns)
        {
            // id 필드는 BaseTable.Row에 이미 정의되어 있으므로 스킵
            if (column.name.ToLower() == "id")
                continue;

            var fieldType = ConvertToCSType(column.type);
            sb.AppendLine($"        public {fieldType} {column.name};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        // 파일 저장
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"Table 클래스 생성: {path}");
    }

    // Data 클래스 생성
    private void GenerateDataClass(SheetData sheetData)
    {
        var className = sheetData.sheetName;
        var tableName = className + "Table";
        var path = $"{_config.DataOutputPath}/{className}.cs";

        // 디렉토리 생성
        if (!Directory.Exists(_config.DataOutputPath))
            Directory.CreateDirectory(_config.DataOutputPath);

        var sb = new StringBuilder();

        // using 문
        sb.AppendLine("using System;");
        sb.AppendLine();

        // 클래스 선언
        sb.AppendLine("[Serializable]");
        sb.AppendLine($"public class {className} : {tableName}.RowData");
        sb.AppendLine("{");
        sb.AppendLine("    // 추가 로직이나 계산된 프로퍼티를 여기에 작성");
        sb.AppendLine("}");

        // 파일 저장
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"Data 클래스 생성: {path}");
    }

    // 서버용 Table 클래스 생성
    private void GenerateServerTableClass(SheetData sheetData)
    {
        var className = sheetData.sheetName + "Table";
        var serverTablePath = Path.GetFullPath(Path.Combine(Application.dataPath, _config.ServerTableOutputPath));
        var path = Path.Combine(serverTablePath, $"{className}.cs");

        // 디렉토리 생성
        if (!Directory.Exists(serverTablePath))
            Directory.CreateDirectory(serverTablePath);

        var sb = new StringBuilder();

        // using 문
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();

        // 네임스페이스
        sb.AppendLine("namespace ProjectQ.Data");
        sb.AppendLine("{");

        // 클래스 선언
        sb.AppendLine($"    public class {className} : BaseTable<{className}.RowData>");
        sb.AppendLine("    {");

        // Row 클래스
        sb.AppendLine("        [Serializable]");
        sb.AppendLine("        public class RowData : Row");
        sb.AppendLine("        {");

        // 필드 생성 (id는 부모 클래스 Row에 이미 존재하므로 제외)
        foreach (var column in sheetData.columns)
        {
            // id 필드는 BaseTable.Row에 이미 정의되어 있으므로 스킵
            if (column.name.ToLower() == "id")
                continue;

            var fieldType = ConvertToCSType(column.type);
            sb.AppendLine($"            public {fieldType} {column.name};");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // 파일 저장
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"서버용 Table 클래스 생성: {path}");
    }

    // ScriptableObject 에셋 생성 및 데이터 채우기
    private void GenerateTableAsset(SheetData sheetData)
    {
        var className = sheetData.sheetName + "Table";
        var assetPath = $"{_config.AssetOutputPath}/{className}.asset";

        // 디렉토리 생성
        if (!Directory.Exists(_config.AssetOutputPath))
            Directory.CreateDirectory(_config.AssetOutputPath);

        // 클래스 타입 가져오기 (모든 어셈블리에서 검색)
        var tableType = FindType(className);
        if (tableType == null)
        {
            Debug.LogError($"클래스를 찾을 수 없습니다: {className}. 컴파일 후 다시 시도하세요.");
            return;
        }

        // ScriptableObject 생성 또는 로드
        var table = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance(tableType);
            AssetDatabase.CreateAsset(table, assetPath);
        }
        else
        {
            // 기존 에셋이 있으면 타입이 맞는지 확인
            if (table.GetType() != tableType)
            {
                AssetDatabase.DeleteAsset(assetPath);
                table = ScriptableObject.CreateInstance(tableType);
                AssetDatabase.CreateAsset(table, assetPath);
            }
        }

        // 데이터 파싱 및 설정
        PopulateTableData(table, sheetData);

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        Debug.Log($"Table 에셋 생성: {assetPath}");
    }

    // 테이블에 데이터 채우기 (Reflection 사용)
    private void PopulateTableData(ScriptableObject table, SheetData sheetData)
    {
        var tableType = table.GetType();
        var rowDataType = tableType.GetNestedType("RowData");

        if (rowDataType == null)
        {
            Debug.LogError($"RowData 타입을 찾을 수 없습니다: {tableType.Name}");
            return;
        }

        var rowsList = Activator.CreateInstance(typeof(List<>).MakeGenericType(rowDataType));
        var addMethod = rowsList.GetType().GetMethod("Add");

        foreach (var rowData in sheetData.rows)
        {
            var row = Activator.CreateInstance(rowDataType);

            // column.index를 사용해 정확한 위치의 데이터 가져오기
            foreach (var column in sheetData.columns)
            {
                var field = rowDataType.GetField(column.name);

                if (field != null && column.index < rowData.Count)
                {
                    var cellValue = rowData[column.index];
                    var value = ConvertValue(cellValue, column.type);
                    field.SetValue(row, value);
                }
            }

            addMethod.Invoke(rowsList, new[] { row });
        }

        // SetRows 메서드 호출
        var setRowsMethod = tableType.GetMethod("SetRows");
        if (setRowsMethod != null)
        {
            setRowsMethod.Invoke(table, new[] { rowsList });
        }
    }

    // 시트 타입을 C# 타입으로 변환
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
                return "string";
        }
    }

    // 문자열 값을 지정된 타입으로 변환
    private object ConvertValue(string value, string type)
    {
        try
        {
            switch (type.ToLower())
            {
                case "int":
                case "integer":
                    return int.Parse(value);
                case "float":
                case "double":
                    return float.Parse(value);
                case "bool":
                case "boolean":
                    return bool.Parse(value);
                case "long":
                    return long.Parse(value);
                default:
                    return value;
            }
        }
        catch
        {
            return GetDefaultValue(type);
        }
    }

    // 타입별 기본값 반환
    private object GetDefaultValue(string type)
    {
        switch (type.ToLower())
        {
            case "int":
            case "integer":
            case "long":
                return 0;
            case "float":
            case "double":
                return 0f;
            case "bool":
            case "boolean":
                return false;
            default:
                return "";
        }
    }

    // 선택한 시트들 파싱
    private void ParseSelectedSheets()
    {
        var selectedCount = 0;
        var pendingSheets = new List<string>();

        foreach (var kvp in _selectedSheets)
        {
            if (kvp.Value)
            {
                var needsCompile = ParseAndGenerateTable(kvp.Key);
                if (needsCompile)
                    pendingSheets.Add(kvp.Key);
                selectedCount++;
            }
        }

        // 컴파일 대기 시트가 있으면 EditorPrefs에 저장
        if (pendingSheets.Count > 0)
        {
            SavePendingSheets(pendingSheets);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{selectedCount}개의 시트 파싱이 완료되었습니다.\n컴파일 후 SO가 자동 생성됩니다.", "확인");
    }

    // 모든 시트 파싱
    private void ParseAllSheets()
    {
        var pendingSheets = new List<string>();

        foreach (var sheetName in _config.SheetNames)
        {
            var needsCompile = ParseAndGenerateTable(sheetName);
            if (needsCompile)
                pendingSheets.Add(sheetName);
        }

        // 컴파일 대기 시트가 있으면 EditorPrefs에 저장
        if (pendingSheets.Count > 0)
        {
            SavePendingSheets(pendingSheets);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{_config.SheetNames.Count}개의 시트 파싱이 완료되었습니다.\n컴파일 후 SO가 자동 생성됩니다.", "확인");
    }

    // 대기 시트 목록 저장
    private void SavePendingSheets(List<string> sheetNames)
    {
        var configPath = AssetDatabase.GetAssetPath(_config);
        EditorPrefs.SetString(PENDING_SHEETS_KEY, string.Join(",", sheetNames));
        EditorPrefs.SetString(PENDING_CONFIG_PATH_KEY, configPath);
        Debug.Log($"컴파일 대기 시트 등록: {string.Join(", ", sheetNames)}");
    }

    // 시트 파싱 및 테이블 생성 (컴파일 필요 여부 반환)
    private bool ParseAndGenerateTable(string sheetName)
    {
        try
        {
            // 1. 시트 데이터 파싱 (CSV 데이터를 가져오면서 서버에도 저장)
            var sheetData = ParseSheet(sheetName);

            if (sheetData.columns.Count == 0)
            {
                Debug.LogError($"파싱 실패: {sheetName}");
                return false;
            }

            // 2. 유니티용 Table 클래스 생성
            GenerateTableClass(sheetData);

            // 3. 유니티용 Data 클래스 생성
            GenerateDataClass(sheetData);

            // 4. 서버용 Table 클래스 생성
            GenerateServerTableClass(sheetData);

            // 5. 클래스가 이미 컴파일되어 있으면 SO 에셋도 생성
            var className = sheetName + "Table";
            var tableType = FindType(className);

            if (tableType != null)
            {
                GenerateTableAsset(sheetData);
                Debug.Log($"{sheetName} 클래스 및 에셋 생성 완료 (유니티 + 서버)");
                return false;
            }
            else
            {
                Debug.Log($"{sheetName} 클래스 생성 완료 (유니티 + 서버). 컴파일 후 SO 자동 생성 예정.");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{sheetName} 파싱 중 오류 발생: {e.Message}");
            return false;
        }
    }

    // 컴파일 후 자동 SO 생성용 정적 메서드
    private static void GenerateTableAssetStatic(string sheetName, SpreadSheetConfig config)
    {
        try
        {
            var className = sheetName + "Table";
            var assetPath = $"{config.AssetOutputPath}/{className}.asset";

            // 디렉토리 생성
            if (!Directory.Exists(config.AssetOutputPath))
                Directory.CreateDirectory(config.AssetOutputPath);

            // 클래스 타입 가져오기
            Type tableType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                tableType = assembly.GetType(className);
                if (tableType != null)
                    break;
            }

            if (tableType == null)
            {
                Debug.LogError($"클래스를 찾을 수 없습니다: {className}");
                return;
            }

            // CSV 데이터 가져오기
            var url = config.GetSheetUrl(sheetName);
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            var csvData = webClient.DownloadString(url);

            // CSV 파싱
            var parsedData = ParseCsvStatic(csvData);
            if (parsedData.Count < 2)
            {
                Debug.LogError($"CSV 데이터가 부족합니다: {sheetName}");
                return;
            }

            // 컬럼 정보 파싱
            var columnNames = parsedData[0];
            var columnTypes = parsedData[1];
            var hasExplicitTypeRow = columnTypes.Exists(t => IsValidExplicitTypeStatic(t.Trim()));

            var columns = new List<(string name, string type, int index)>();
            for (int i = 0; i < columnNames.Count; i++)
            {
                var columnName = columnNames[i].Trim();
                if (string.IsNullOrEmpty(columnName))
                    continue;

                string name, type;
                if (hasExplicitTypeRow)
                {
                    var columnType = i < columnTypes.Count ? columnTypes[i].Trim().ToLower() : "string";
                    name = SanitizeIdentifierStatic(columnName);
                    type = IsValidExplicitTypeStatic(columnType) ? columnType : "string";
                }
                else
                {
                    (name, type) = ExtractNameAndTypeStatic(columnName);
                    name = SanitizeIdentifierStatic(name);
                }

                if (!string.IsNullOrEmpty(name))
                    columns.Add((name, type, i));
            }

            // ScriptableObject 생성 또는 로드
            var table = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance(tableType);
                AssetDatabase.CreateAsset(table, assetPath);
            }
            else if (table.GetType() != tableType)
            {
                AssetDatabase.DeleteAsset(assetPath);
                table = ScriptableObject.CreateInstance(tableType);
                AssetDatabase.CreateAsset(table, assetPath);
            }

            // 데이터 채우기
            var rowDataType = tableType.GetNestedType("RowData");
            if (rowDataType == null)
            {
                Debug.LogError($"RowData 타입을 찾을 수 없습니다: {tableType.Name}");
                return;
            }

            var rowsList = Activator.CreateInstance(typeof(List<>).MakeGenericType(rowDataType));
            var addMethod = rowsList.GetType().GetMethod("Add");

            var dataStartRow = hasExplicitTypeRow ? 2 : 1;
            for (int i = dataStartRow; i < parsedData.Count; i++)
            {
                var rowData = parsedData[i];
                var row = Activator.CreateInstance(rowDataType);

                foreach (var (colName, colType, colIndex) in columns)
                {
                    var field = rowDataType.GetField(colName);
                    if (field != null && colIndex < rowData.Count)
                    {
                        var value = ConvertValueStatic(rowData[colIndex], colType);
                        field.SetValue(row, value);
                    }
                }

                addMethod.Invoke(rowsList, new[] { row });
            }

            // SetRows 메서드 호출
            var setRowsMethod = tableType.GetMethod("SetRows");
            if (setRowsMethod != null)
            {
                setRowsMethod.Invoke(table, new[] { rowsList });
            }

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            Debug.Log($"SO 생성 완료: {assetPath} ({parsedData.Count - dataStartRow}개 행)");
        }
        catch (Exception e)
        {
            Debug.LogError($"{sheetName} SO 생성 중 오류: {e.Message}");
        }
    }

    // 정적 CSV 파싱 메서드
    private static List<List<string>> ParseCsvStatic(string csvData)
    {
        var result = new List<List<string>>();
        if (string.IsNullOrEmpty(csvData)) return result;

        csvData = csvData.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd() + "\n";

        var delimiter = csvData.Split('\n')[0].Split('\t').Length - 1 >
                        csvData.Split('\n')[0].Split(',').Length - 1 ? '\t' : ',';

        var lines = new List<string>();
        var currentLine = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < csvData.Length; i++)
        {
            var c = csvData[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < csvData.Length && csvData[i + 1] == '"')
                {
                    currentLine.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == '\n' && !inQuotes)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
            else
            {
                currentLine.Append(c);
            }
        }
        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cells = new List<string>();
            var currentCell = new StringBuilder();
            inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentCell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    cells.Add(currentCell.ToString());
                    currentCell.Clear();
                }
                else
                {
                    currentCell.Append(c);
                }
            }
            cells.Add(currentCell.ToString());

            if (cells.Count > 0)
                result.Add(cells);
        }

        return result;
    }

    // 정적 타입 검증 메서드
    private static bool IsValidExplicitTypeStatic(string type)
    {
        if (string.IsNullOrEmpty(type)) return false;
        var validTypes = new[] { "int", "integer", "string", "text", "float", "double", "bool", "boolean", "long" };
        return Array.Exists(validTypes, t => t == type.ToLower());
    }

    // 정적 식별자 정리 메서드
    private static string SanitizeIdentifierStatic(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
        }
        var result = sb.ToString();
        if (result.Length > 0 && char.IsDigit(result[0]))
            result = "_" + result;
        return result;
    }

    // 정적 타입 추출 메서드
    private static (string name, string type) ExtractNameAndTypeStatic(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return (columnName, "string");

        var typeSuffixes = new[]
        {
            ("string", "string"), ("float", "float"), ("double", "float"),
            ("boolean", "bool"), ("bool", "bool"), ("integer", "int"),
            ("long", "long"), ("int", "int")
        };

        var lowerName = columnName.ToLower();
        foreach (var (suffix, csType) in typeSuffixes)
        {
            if (lowerName.EndsWith(suffix))
            {
                var name = columnName.Substring(0, columnName.Length - suffix.Length);
                if (!string.IsNullOrEmpty(name))
                    return (name, csType);
            }
        }
        return (columnName, "string");
    }

    // 정적 값 변환 메서드
    private static object ConvertValueStatic(string value, string type)
    {
        try
        {
            switch (type.ToLower())
            {
                case "int":
                case "integer":
                    return int.Parse(value);
                case "float":
                case "double":
                    return float.Parse(value);
                case "bool":
                case "boolean":
                    return bool.Parse(value);
                case "long":
                    return long.Parse(value);
                default:
                    return value;
            }
        }
        catch
        {
            switch (type.ToLower())
            {
                case "int":
                case "integer":
                case "long":
                    return 0;
                case "float":
                case "double":
                    return 0f;
                case "bool":
                case "boolean":
                    return false;
                default:
                    return "";
            }
        }
    }

    // 모든 어셈블리에서 타입 찾기
    private Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }

    /*
    시트 목록에 없는 테이블/데이터 스크립트 정리
    Table 스크립트, Data 스크립트, SO 에셋, 서버용 스크립트 모두 정리
    */
    private void CleanupUnusedScripts()
    {
        // 현재 시트 이름 목록 가져오기
        var validSheetNames = new HashSet<string>();
        foreach (var sheetName in _config.SheetNames)
        {
            validSheetNames.Add(sheetName);
        }

        var deletedFiles = new List<string>();

        // 1. Table 스크립트 정리 (Assets/Script/Data/Table/)
        var tableScripts = GetExistingTableScripts(_config.TableOutputPath);
        foreach (var scriptPath in tableScripts)
        {
            var fileName = Path.GetFileNameWithoutExtension(scriptPath);

            // BaseTable.cs, SpreadSheetConfig.cs는 제외
            if (fileName == "BaseTable" || fileName == "SpreadSheetConfig")
                continue;

            // Table 접미사 제거하여 시트 이름 추출
            var sheetName = fileName.EndsWith("Table")
                ? fileName.Substring(0, fileName.Length - 5)
                : fileName;

            if (!validSheetNames.Contains(sheetName))
            {
                deletedFiles.Add(scriptPath);
            }
        }

        // 2. Data 스크립트 정리 (Assets/Script/Data/TableData/)
        var dataScripts = GetExistingTableScripts(_config.DataOutputPath);
        foreach (var scriptPath in dataScripts)
        {
            var fileName = Path.GetFileNameWithoutExtension(scriptPath);

            if (!validSheetNames.Contains(fileName))
            {
                deletedFiles.Add(scriptPath);
            }
        }

        // 3. SO 에셋 정리 (Assets/Datas/)
        var soAssets = GetExistingTableAssets(_config.AssetOutputPath);
        foreach (var assetPath in soAssets)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);

            // Table 접미사 제거하여 시트 이름 추출
            var sheetName = fileName.EndsWith("Table")
                ? fileName.Substring(0, fileName.Length - 5)
                : fileName;

            if (!validSheetNames.Contains(sheetName))
            {
                deletedFiles.Add(assetPath);
            }
        }

        // 4. 서버용 Table 스크립트 정리
        var serverTablePath = Path.GetFullPath(Path.Combine(Application.dataPath, _config.ServerTableOutputPath));
        if (Directory.Exists(serverTablePath))
        {
            var serverScripts = GetExistingTableScripts(serverTablePath);
            foreach (var scriptPath in serverScripts)
            {
                var fileName = Path.GetFileNameWithoutExtension(scriptPath);

                // BaseTable.cs, CsvParser.cs는 제외
                if (fileName == "BaseTable" || fileName == "CsvParser")
                    continue;

                var sheetName = fileName.EndsWith("Table")
                    ? fileName.Substring(0, fileName.Length - 5)
                    : fileName;

                if (!validSheetNames.Contains(sheetName))
                {
                    deletedFiles.Add(scriptPath);
                }
            }
        }

        // 5. 서버용 CSV 파일 정리
        var serverCsvPath = Path.GetFullPath(Path.Combine(Application.dataPath, _config.ServerCsvOutputPath));
        if (Directory.Exists(serverCsvPath))
        {
            var csvFiles = Directory.GetFiles(serverCsvPath, "*.csv");
            foreach (var csvPath in csvFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(csvPath);

                if (!validSheetNames.Contains(fileName))
                {
                    deletedFiles.Add(csvPath);
                }
            }
        }

        // 삭제 확인 대화상자
        if (deletedFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("정리 완료", "삭제할 파일이 없습니다.", "확인");
            return;
        }

        var fileListMessage = string.Join("\n", deletedFiles.ConvertAll(f => "- " + Path.GetFileName(f)));
        var confirmMessage = $"다음 {deletedFiles.Count}개의 파일을 삭제하시겠습니까?\n\n{fileListMessage}";

        if (!EditorUtility.DisplayDialog("파일 삭제 확인", confirmMessage, "삭제", "취소"))
        {
            return;
        }

        // 파일 삭제 실행
        var deletedCount = 0;
        foreach (var filePath in deletedFiles)
        {
            try
            {
                // Unity 프로젝트 내 파일인지 확인
                if (filePath.StartsWith(Application.dataPath))
                {
                    // Assets/ 상대 경로로 변환
                    var relativePath = "Assets" + filePath.Substring(Application.dataPath.Length);
                    AssetDatabase.DeleteAsset(relativePath);
                }
                else
                {
                    // 서버 프로젝트 파일은 직접 삭제
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                deletedCount++;
                Debug.Log($"[TableParser] 삭제됨: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TableParser] 삭제 실패: {filePath}\n{e.Message}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("정리 완료", $"{deletedCount}개의 파일이 삭제되었습니다.", "확인");
    }

    // 지정 경로에서 기존 테이블 스크립트 목록 가져오기
    private List<string> GetExistingTableScripts(string folderPath)
    {
        var scripts = new List<string>();

        if (!Directory.Exists(folderPath))
            return scripts;

        var files = Directory.GetFiles(folderPath, "*.cs");
        scripts.AddRange(files);

        return scripts;
    }

    // 지정 경로에서 기존 SO 에셋 목록 가져오기
    private List<string> GetExistingTableAssets(string folderPath)
    {
        var assets = new List<string>();

        if (!Directory.Exists(folderPath))
            return assets;

        var files = Directory.GetFiles(folderPath, "*.asset");
        assets.AddRange(files);

        return assets;
    }

    // 시트 데이터 구조
    private class SheetData
    {
        public string sheetName;
        public List<ColumnInfo> columns = new List<ColumnInfo>();
        public List<List<string>> rows = new List<List<string>>();
    }

    // 컬럼 정보
    private class ColumnInfo
    {
        public string name;
        public string type;
        public int index;
    }
}
