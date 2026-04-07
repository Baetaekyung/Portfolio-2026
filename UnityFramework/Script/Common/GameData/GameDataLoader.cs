using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/*
테이블 SO들을 관리하고 접근할 수 있는 싱글톤 클래스
테이블 자동 수집, 접근, 서버 데이터 동기화 기능 제공
버전 + 델타 방식으로 효율적인 데이터 동기화 지원
*/
[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class GameDataLoader : Singleton<GameDataLoader>
{
    // 테이블 SO 저장 경로
    private const string TABLE_PATH = "Tables";

    // 타입별 테이블 캐싱
    private Dictionary<Type, ScriptableObject> _tableCache = new Dictionary<Type, ScriptableObject>();

    // 테이블 이름별 캐싱
    private Dictionary<string, ScriptableObject> _tableNameCache = new Dictionary<string, ScriptableObject>();

    // 테이블별 버전 정보 캐싱
    private Dictionary<string, string> _versionCache = new Dictionary<string, string>();

    // 초기화 완료 여부
    private bool _isInitialized = false;
    public bool IsInitialized => _isInitialized;

    // 초기화 완료 이벤트
    public event Action OnInitialized;

    // 동기화 완료 이벤트
    public event Action<string, SyncResult> OnTableSynced;

    protected override void OnCreated()
    {
        base.OnCreated();
        LoadAllTables();
    }

    /*
    Resources 폴더에서 모든 테이블 SO를 자동으로 수집
    Assets/Resources/Datas/ 경로에서 로드
    */
    public void LoadAllTables()
    {
        _tableCache.Clear();
        _tableNameCache.Clear();
        _versionCache.Clear();

        // Resources/Datas 폴더에서 모든 ScriptableObject 로드
        var tables = Resources.LoadAll<ScriptableObject>(TABLE_PATH);

        foreach (var table in tables)
        {
            var tableType = table.GetType();

            // BaseTable을 상속받은 타입인지 확인
            if (IsBaseTableType(tableType))
            {
                _tableCache[tableType] = table;
                _tableNameCache[table.name] = table;

                // 버전 정보 캐싱
                var versionProp = tableType.GetProperty("Version");
                if (versionProp != null)
                {
                    var version = versionProp.GetValue(table) as string ?? "1.0.0";
                    _versionCache[table.name] = version;
                }

                Debug.Log($"[GameDataLoader] 테이블 로드: {table.name} (v{_versionCache.GetValueOrDefault(table.name, "1.0.0")})");
            }
        }

        _isInitialized = true;
        Debug.Log($"[GameDataLoader] 총 {_tableCache.Count}개의 테이블 로드 완료");

        OnInitialized?.Invoke();
    }

    // BaseTable<T>를 상속받은 타입인지 확인
    private bool IsBaseTableType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name.StartsWith("BaseTable"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    /*
    타입으로 테이블 가져오기
    예: var itemTable = GameDataLoader.Instance.GetTable<ItemTable>();
    */
    public T GetTable<T>() where T : ScriptableObject
    {
        var type = typeof(T);

        if (_tableCache.TryGetValue(type, out var table))
        {
            return table as T;
        }

        // 캐시에 없으면 직접 로드 시도
        var tableName = type.Name;
        var loadedTable = Resources.Load<T>($"{TABLE_PATH}/{tableName}");

        if (loadedTable != null)
        {
            _tableCache[type] = loadedTable;
            _tableNameCache[tableName] = loadedTable;
            return loadedTable;
        }

        Debug.LogWarning($"[GameDataLoader] 테이블을 찾을 수 없습니다: {type.Name}");
        return null;
    }

    /*
    이름으로 테이블 가져오기
    예: var table = GameDataLoader.Instance.GetTableByName("ItemTable");
    */
    public ScriptableObject GetTableByName(string tableName)
    {
        if (_tableNameCache.TryGetValue(tableName, out var table))
        {
            return table;
        }

        // 캐시에 없으면 직접 로드 시도
        var loadedTable = Resources.Load<ScriptableObject>($"{TABLE_PATH}/{tableName}");

        if (loadedTable != null)
        {
            _tableNameCache[tableName] = loadedTable;
            _tableCache[loadedTable.GetType()] = loadedTable;
            return loadedTable;
        }

        Debug.LogWarning($"[GameDataLoader] 테이블을 찾을 수 없습니다: {tableName}");
        return null;
    }

    // 테이블 버전 가져오기
    public string GetTableVersion(string tableName)
    {
        if (_versionCache.TryGetValue(tableName, out var version))
            return version;

        var table = GetTableByName(tableName);
        if (table != null)
        {
            var versionProp = table.GetType().GetProperty("Version");
            if (versionProp != null)
            {
                return versionProp.GetValue(table) as string ?? "1.0.0";
            }
        }

        return "1.0.0";
    }

    // 모든 테이블 버전 정보 가져오기
    public Dictionary<string, string> GetAllTableVersions()
    {
        return new Dictionary<string, string>(_versionCache);
    }

    /*
    서버 데이터와 동기화 (버전 + 델타 방식)
    서버에서 받은 TableSyncData로 로컬 테이블 갱신
    */
    public SyncResult SyncOnServerData(string tableName, TableSyncData syncData)
    {
        var table = GetTableByName(tableName);
        if (table == null)
        {
            Debug.LogError($"[GameDataLoader] 동기화 실패 - 테이블을 찾을 수 없습니다: {tableName}");
            return new SyncResult { Success = false, Message = "테이블을 찾을 수 없습니다" };
        }

        var result = new SyncResult { TableName = tableName };

        try
        {
            var currentVersion = GetTableVersion(tableName);

            // 버전이 같으면 동기화 불필요
            if (currentVersion == syncData.Version)
            {
                result.Success = true;
                result.SyncType = ESyncType.NO_CHANGE;
                result.Message = "이미 최신 버전입니다";
                Debug.Log($"[GameDataLoader] {tableName}: 이미 최신 버전 (v{currentVersion})");
                return result;
            }

            // 델타 업데이트 가능 여부 확인
            if (syncData.SupportsDelta && syncData.DeltaData != null)
            {
                // 델타 업데이트 적용
                result = ApplyDeltaUpdate(table, syncData);
            }
            else
            {
                // 전체 교체
                result = ApplyFullUpdate(table, syncData);
            }

            // 버전 캐시 업데이트
            if (result.Success)
            {
                _versionCache[tableName] = syncData.Version;
            }

            OnTableSynced?.Invoke(tableName, result);
            return result;
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Message = $"동기화 중 오류: {e.Message}";
            Debug.LogError($"[GameDataLoader] {tableName} 동기화 중 오류: {e.Message}");
            return result;
        }
    }

    /*
    여러 테이블 일괄 동기화
    */
    public Dictionary<string, SyncResult> SyncOnServerData(Dictionary<string, TableSyncData> syncDataMap)
    {
        var results = new Dictionary<string, SyncResult>();

        foreach (var kvp in syncDataMap)
        {
            results[kvp.Key] = SyncOnServerData(kvp.Key, kvp.Value);
        }

        return results;
    }

    // 델타 업데이트 적용
    private SyncResult ApplyDeltaUpdate(ScriptableObject table, TableSyncData syncData)
    {
        var result = new SyncResult
        {
            TableName = table.name,
            SyncType = ESyncType.DELTA,
            PreviousVersion = GetTableVersion(table.name),
            NewVersion = syncData.Version
        };

        var tableType = table.GetType();
        var delta = syncData.DeltaData;

        // RowData 타입 가져오기
        var rowDataType = tableType.GetNestedType("RowData");
        if (rowDataType == null)
        {
            result.Success = false;
            result.Message = "RowData 타입을 찾을 수 없습니다";
            return result;
        }

        // 추가/수정/삭제 데이터 파싱
        var addedRows = ParseCsvToRowList(delta.AddedRowsCsv, rowDataType);
        var modifiedRows = ParseCsvToRowList(delta.ModifiedRowsCsv, rowDataType);
        var deletedIds = delta.DeletedIds ?? new List<int>();

        // HotFixDelta 메서드 호출
        var hotFixDeltaMethod = tableType.GetMethod("HotFixDelta");
        if (hotFixDeltaMethod != null)
        {
            hotFixDeltaMethod.Invoke(table, new object[] { addedRows, modifiedRows, deletedIds, syncData.Version });

            result.Success = true;
            result.AddedCount = GetListCount(addedRows);
            result.ModifiedCount = GetListCount(modifiedRows);
            result.DeletedCount = deletedIds.Count;
            result.Message = $"델타 업데이트 완료 (추가:{result.AddedCount}, 수정:{result.ModifiedCount}, 삭제:{result.DeletedCount})";

            Debug.Log($"[GameDataLoader] {table.name}: {result.Message}");
        }
        else
        {
            result.Success = false;
            result.Message = "HotFixDelta 메서드를 찾을 수 없습니다";
        }

        return result;
    }

    // 전체 교체 적용
    private SyncResult ApplyFullUpdate(ScriptableObject table, TableSyncData syncData)
    {
        var result = new SyncResult
        {
            TableName = table.name,
            SyncType = ESyncType.FULL,
            PreviousVersion = GetTableVersion(table.name),
            NewVersion = syncData.Version
        };

        var tableType = table.GetType();

        // RowData 타입 가져오기
        var rowDataType = tableType.GetNestedType("RowData");
        if (rowDataType == null)
        {
            result.Success = false;
            result.Message = "RowData 타입을 찾을 수 없습니다";
            return result;
        }

        // CSV 데이터 파싱
        var newRows = ParseCsvToRowList(syncData.FullDataCsv, rowDataType);

        // HotFix 메서드 호출
        var hotFixMethod = tableType.GetMethod("HotFix");
        if (hotFixMethod != null)
        {
            hotFixMethod.Invoke(table, new object[] { newRows, syncData.Version });

            result.Success = true;
            result.AddedCount = GetListCount(newRows);
            result.Message = $"전체 교체 완료 ({result.AddedCount}개 행)";

            Debug.Log($"[GameDataLoader] {table.name}: 전체 교체 완료 (v{result.PreviousVersion} → v{result.NewVersion})");
        }
        else
        {
            result.Success = false;
            result.Message = "HotFix 메서드를 찾을 수 없습니다";
        }

        return result;
    }

    // CSV를 Row 리스트로 파싱
    private object ParseCsvToRowList(string csvData, Type rowDataType)
    {
        if (string.IsNullOrEmpty(csvData))
            return null;

        var listType = typeof(List<>).MakeGenericType(rowDataType);
        var rowsList = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");

        var lines = ParseCsvLines(csvData);
        if (lines.Count < 2)
            return rowsList;

        var columnNames = lines[0];
        var columnTypes = lines[1];
        var hasTypeRow = IsValidTypeRow(columnTypes);
        var dataStartRow = hasTypeRow ? 2 : 1;

        // 컬럼 정보 구성
        var columns = new List<(string name, string type, int index, FieldInfo field)>();
        for (int i = 0; i < columnNames.Count; i++)
        {
            var columnName = columnNames[i].Trim();
            if (string.IsNullOrEmpty(columnName))
                continue;

            string name, type;
            if (hasTypeRow)
            {
                name = SanitizeIdentifier(columnName);
                type = i < columnTypes.Count ? columnTypes[i].Trim().ToLower() : "string";
            }
            else
            {
                (name, type) = ExtractNameAndType(columnName);
                name = SanitizeIdentifier(name);
            }

            var field = rowDataType.GetField(name);
            if (field != null)
            {
                columns.Add((name, type, i, field));
            }
        }

        // 데이터 파싱
        for (int i = dataStartRow; i < lines.Count; i++)
        {
            var rowData = lines[i];
            var row = Activator.CreateInstance(rowDataType);

            foreach (var (colName, colType, colIndex, field) in columns)
            {
                if (colIndex < rowData.Count)
                {
                    var value = ConvertValue(rowData[colIndex], colType);
                    field.SetValue(row, value);
                }
            }

            addMethod.Invoke(rowsList, new[] { row });
        }

        return rowsList;
    }

    #region CSV 파싱 유틸리티

    private List<List<string>> ParseCsvLines(string csvData)
    {
        var result = new List<List<string>>();
        csvData = csvData.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd() + "\n";

        var delimiter = DetectDelimiter(csvData);
        var lines = new List<string>();
        var currentLine = new System.Text.StringBuilder();
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
            var currentCell = new System.Text.StringBuilder();
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

    private char DetectDelimiter(string csvData)
    {
        var firstLine = csvData.Split('\n')[0];
        var tabCount = firstLine.Split('\t').Length - 1;
        var commaCount = firstLine.Split(',').Length - 1;
        return tabCount > commaCount ? '\t' : ',';
    }

    private bool IsValidTypeRow(List<string> row)
    {
        var validTypes = new[] { "int", "integer", "string", "text", "float", "double", "bool", "boolean", "long" };
        foreach (var cell in row)
        {
            if (Array.Exists(validTypes, t => t == cell.Trim().ToLower()))
                return true;
        }
        return false;
    }

    private string SanitizeIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var sb = new System.Text.StringBuilder();
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

    private (string name, string type) ExtractNameAndType(string columnName)
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

    #endregion

    #region 유틸리티 메서드

    // 캐시된 모든 테이블 목록 반환
    public IReadOnlyCollection<ScriptableObject> GetAllTables()
    {
        return _tableCache.Values;
    }

    // 특정 테이블이 로드되어 있는지 확인
    public bool HasTable<T>() where T : ScriptableObject
    {
        return _tableCache.ContainsKey(typeof(T));
    }

    // 특정 테이블 다시 로드
    public void ReloadTable<T>() where T : ScriptableObject
    {
        var type = typeof(T);
        var tableName = type.Name;

        _tableCache.Remove(type);
        _tableNameCache.Remove(tableName);
        _versionCache.Remove(tableName);

        GetTable<T>();
    }

    // 모든 테이블 다시 로드
    public void ReloadAllTables()
    {
        LoadAllTables();
    }

    // object 타입의 리스트에서 Count 가져오기
    private int GetListCount(object list)
    {
        if (list == null)
            return 0;

        if (list is System.Collections.IList iList)
            return iList.Count;

        return 0;
    }

    #endregion
}

#region 동기화 관련 데이터 구조

// 동기화 타입
public enum ESyncType
{
    NO_CHANGE,  // 변경 없음
    DELTA,      // 델타 업데이트
    FULL        // 전체 교체
}

// 서버에서 받는 테이블 동기화 데이터
[Serializable]
public class TableSyncData
{
    // 테이블 버전
    public string Version;

    // 델타 업데이트 지원 여부
    public bool SupportsDelta;

    // 전체 데이터 (델타 미지원 또는 버전 차이가 큰 경우)
    public string FullDataCsv;

    // 델타 데이터
    public DeltaData DeltaData;
}

// 델타 업데이트 데이터
[Serializable]
public class DeltaData
{
    // 추가된 행들 (CSV 형식)
    public string AddedRowsCsv;

    // 수정된 행들 (CSV 형식)
    public string ModifiedRowsCsv;

    // 삭제된 행 ID 목록
    public List<int> DeletedIds;
}

// 동기화 결과
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

#endregion
