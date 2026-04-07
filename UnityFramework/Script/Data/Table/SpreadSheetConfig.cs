using System.Collections.Generic;
using UnityEngine;

/*
GoogleSpreadSheet 연동에 필요한 설정 정보를 저장하는 ScriptableObject
TableParser에서 이 설정을 참조하여 데이터 파싱 수행
웹에 게시된 스프레드시트에서 CSV 형식으로 데이터를 가져옴
*/
[CreateAssetMenu(fileName = "SpreadSheetConfig", menuName = "Data/SpreadSheetConfig")]
public class SpreadSheetConfig : ScriptableObject
{
    [Header("GoogleSpreadSheet 설정")]
    [SerializeField]
    private string spreadsheetId;

    [Header("시트 목록")]
    [SerializeField]
    private List<string> sheetNames = new List<string>();

    [Header("유니티 출력 경로 설정")]
    [SerializeField]
    private string tableOutputPath = "Assets/Script/Data/Table";

    [SerializeField]
    private string dataOutputPath = "Assets/Script/Data/TableData";

    [SerializeField]
    private string assetOutputPath = "Assets/Datas";

    [Header("서버 출력 경로 설정")]
    [SerializeField]
    private string serverCsvOutputPath = "../Project_Q_Server/Datas";

    [SerializeField]
    private string serverTableOutputPath = "../Project_Q_Server/Data/Table";

    // 프로퍼티
    public string SpreadsheetId => spreadsheetId;
    public IReadOnlyList<string> SheetNames => sheetNames;
    public string TableOutputPath => tableOutputPath;
    public string DataOutputPath => dataOutputPath;
    public string AssetOutputPath => assetOutputPath;
    public string ServerCsvOutputPath => serverCsvOutputPath;
    public string ServerTableOutputPath => serverTableOutputPath;

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
