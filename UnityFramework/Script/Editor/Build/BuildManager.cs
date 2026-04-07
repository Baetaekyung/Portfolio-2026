using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// 빌드 자동화를 관리하는 EditorWindow
/// BuildConfig를 직접 주입하여 빌드 실행
/// </summary>
public class BuildManager : EditorWindow
{
    private const string BUILD_FOLDER_NAME = "Builds";

    // 주입받을 BuildConfig
    private BuildConfig _buildConfig;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Build/Build Window", false, 100)]
    public static void ShowWindow()
    {
        var window = GetWindow<BuildManager>("Build Manager");
        window.minSize = new Vector2(400, 500);
    }

    [MenuItem("Tools/Build/Open Build Folder", false, 200)]
    public static void OpenBuildFolder()
    {
        var buildPath = GetBuildFolderPath();

        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        EditorUtility.RevealInFinder(buildPath);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawHeader();
        DrawConfigField();

        if (_buildConfig != null)
        {
            DrawConfigPreview();
            DrawQuickSettings();
            DrawBuildButtons();
        }
        else
        {
            DrawNoConfigMessage();
        }

        EditorGUILayout.EndScrollView();
    }

    #region GUI 그리기

    /// <summary>
    /// 헤더 영역
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Build Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
    }

    /// <summary>
    /// BuildConfig 할당 필드
    /// </summary>
    private void DrawConfigField()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Build Config", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // BuildConfig 오브젝트 필드
        _buildConfig = (BuildConfig)EditorGUILayout.ObjectField(
            "Config 파일",
            _buildConfig,
            typeof(BuildConfig),
            false
        );

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    /// <summary>
    /// Config 미할당 시 메시지
    /// </summary>
    private void DrawNoConfigMessage()
    {
        EditorGUILayout.HelpBox(
            "BuildConfig를 할당해주세요.\n\n" +
            "BuildConfig 생성 방법:\n" +
            "Project 창 우클릭 > Create > Build > BuildConfig",
            MessageType.Info
        );
    }

    /// <summary>
    /// 현재 Config 설정 미리보기
    /// </summary>
    private void DrawConfigPreview()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("현재 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 읽기 전용으로 현재 설정 표시
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.EnumPopup("플랫폼", _buildConfig.buildTargetType);
        EditorGUILayout.EnumPopup("출력 형식", _buildConfig.buildOutputType);
        EditorGUILayout.Toggle("개발 빌드", _buildConfig.developmentBuild);
        EditorGUILayout.Toggle("디버깅 허용", _buildConfig.allowDebugging);

        if (!string.IsNullOrEmpty(_buildConfig.defineSymbols))
        {
            EditorGUILayout.LabelField("Define Symbols", _buildConfig.defineSymbols);
        }
        EditorGUI.EndDisabledGroup();

        // Config 수정 버튼
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Config 수정하기"))
        {
            Selection.activeObject = _buildConfig;
            EditorGUIUtility.PingObject(_buildConfig);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    /// <summary>
    /// 빠른 설정 변경 버튼
    /// </summary>
    private void DrawQuickSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("빠른 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        // Android 설정 버튼들
        if (GUILayout.Button("Android APK"))
        {
            SetAndroidAPK();
        }
        if (GUILayout.Button("Android AAB"))
        {
            SetAndroidAAB();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        // iOS 설정 버튼들
        if (GUILayout.Button("iOS"))
        {
            SetIOS();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    /// <summary>
    /// 빌드 실행 버튼
    /// </summary>
    private void DrawBuildButtons()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("빌드 실행", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 빌드 경로 표시
        var outputPath = GetBuildOutputPath(_buildConfig);
        EditorGUILayout.LabelField("출력 경로", outputPath);

        EditorGUILayout.Space(10);

        // 빌드 버튼
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("빌드 시작", GUILayout.Height(40)))
        {
            StartBuild();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // 빌드 폴더 열기
        if (GUILayout.Button("빌드 폴더 열기"))
        {
            OpenBuildFolder();
        }

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region 빠른 설정 메서드

    private void SetAndroidAPK()
    {
        if (_buildConfig == null) return;

        _buildConfig.buildTargetType = BuildConfig.BuildTargetType.ANDROID;
        _buildConfig.buildOutputType = BuildConfig.BuildOutputType.APK;
        _buildConfig.useAppBundle = false;
        SaveConfig();
        Debug.Log("[BuildManager] Android APK로 설정되었습니다.");
    }

    private void SetAndroidAAB()
    {
        if (_buildConfig == null) return;

        _buildConfig.buildTargetType = BuildConfig.BuildTargetType.ANDROID;
        _buildConfig.buildOutputType = BuildConfig.BuildOutputType.AAB;
        _buildConfig.useAppBundle = true;
        SaveConfig();
        Debug.Log("[BuildManager] Android AAB로 설정되었습니다.");
    }

    private void SetIOS()
    {
        if (_buildConfig == null) return;

        _buildConfig.buildTargetType = BuildConfig.BuildTargetType.IOS;
        SaveConfig();
        Debug.Log("[BuildManager] iOS로 설정되었습니다.");
    }

    private void SaveConfig()
    {
        EditorUtility.SetDirty(_buildConfig);
        AssetDatabase.SaveAssets();
        Repaint();
    }

    #endregion

    #region 빌드 실행

    private void StartBuild()
    {
        if (_buildConfig == null)
        {
            EditorUtility.DisplayDialog("오류", "BuildConfig가 할당되지 않았습니다.", "확인");
            return;
        }

        // 빌드 확인 대화상자
        var message = $"빌드를 시작하시겠습니까?\n\n" +
                      $"플랫폼: {_buildConfig.buildTargetType}\n" +
                      $"출력 형식: {_buildConfig.buildOutputType}\n" +
                      $"개발 빌드: {_buildConfig.developmentBuild}";

        if (!EditorUtility.DisplayDialog("빌드 확인", message, "빌드 시작", "취소"))
        {
            return;
        }

        ExecuteBuild(_buildConfig);
    }

    /// <summary>
    /// 실제 빌드 실행
    /// </summary>
    private void ExecuteBuild(BuildConfig config)
    {
        // Define Symbols 적용
        ApplyDefineSymbols(config);

        // 빌드 경로 설정
        var buildPath = GetBuildOutputPath(config);

        // 빌드할 씬 목록
        var scenes = GetBuildScenes(config);

        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("빌드 오류", "빌드할 씬이 없습니다.", "확인");
            return;
        }

        // Android 빌드 설정
        if (config.buildTargetType == BuildConfig.BuildTargetType.ANDROID)
        {
            ConfigureAndroidBuild(config);
        }

        Debug.Log($"[BuildManager] 빌드 시작: {buildPath}");

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = config.GetBuildTarget(),
            options = config.GetBuildOptions()
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        // 빌드 결과 처리
        switch (summary.result)
        {
            case BuildResult.Succeeded:
                var fileSize = GetFileSizeString(summary.totalSize);
                Debug.Log($"[BuildManager] 빌드 성공! 크기: {fileSize}, 시간: {summary.totalTime}");
                EditorUtility.DisplayDialog("빌드 완료",
                    $"빌드가 성공적으로 완료되었습니다.\n\n경로: {buildPath}\n크기: {fileSize}", "확인");
                EditorUtility.RevealInFinder(buildPath);
                break;

            case BuildResult.Failed:
                Debug.LogError($"[BuildManager] 빌드 실패! 오류 수: {summary.totalErrors}");
                EditorUtility.DisplayDialog("빌드 실패",
                    $"빌드에 실패했습니다.\n오류 수: {summary.totalErrors}\n\n콘솔을 확인하세요.", "확인");
                break;

            case BuildResult.Cancelled:
                Debug.Log("[BuildManager] 빌드가 취소되었습니다.");
                break;
        }
    }

    /// <summary>
    /// Android 빌드 설정 적용
    /// </summary>
    private void ConfigureAndroidBuild(BuildConfig config)
    {
        EditorUserBuildSettings.buildAppBundle =
            config.useAppBundle || config.buildOutputType == BuildConfig.BuildOutputType.AAB;
        PlayerSettings.Android.minifyRelease = config.minifyRelease;
    }

    /// <summary>
    /// Define Symbols 적용
    /// </summary>
    private void ApplyDefineSymbols(BuildConfig config)
    {
        if (string.IsNullOrEmpty(config.defineSymbols)) return;

        // NamedBuildTarget 사용 (Unity 2021.2+)
        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(config.GetBuildTargetGroup());
        PlayerSettings.SetScriptingDefineSymbols(namedTarget, config.defineSymbols);
        Debug.Log($"[BuildManager] Define Symbols 적용: {config.defineSymbols}");
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 빌드 폴더 경로 반환
    /// </summary>
    private static string GetBuildFolderPath()
    {
        var projectPath = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectPath, BUILD_FOLDER_NAME);
    }

    /// <summary>
    /// 빌드 출력 경로 반환
    /// </summary>
    private static string GetBuildOutputPath(BuildConfig config)
    {
        var buildFolder = GetBuildFolderPath();

        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var productName = PlayerSettings.productName;

        string fileName;
        if (config.buildTargetType == BuildConfig.BuildTargetType.ANDROID)
        {
            var extension = config.buildOutputType == BuildConfig.BuildOutputType.AAB ? "aab" : "apk";
            fileName = $"{productName}_{timestamp}.{extension}";
        }
        else
        {
            fileName = $"{productName}_{timestamp}_iOS";
        }

        return Path.Combine(buildFolder, fileName);
    }

    /// <summary>
    /// 빌드에 포함할 씬 목록 반환
    /// </summary>
    private static string[] GetBuildScenes(BuildConfig config)
    {
        if (config.buildScenes != null && config.buildScenes.Length > 0)
        {
            return config.buildScenes
                .Where(scene => scene != null)
                .Select(scene => AssetDatabase.GetAssetPath(scene))
                .ToArray();
        }

        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    /// <summary>
    /// 파일 크기 문자열 변환
    /// </summary>
    private static string GetFileSizeString(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
