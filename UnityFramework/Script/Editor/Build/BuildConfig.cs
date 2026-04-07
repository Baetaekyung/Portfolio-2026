using UnityEngine;
using UnityEditor;

/// <summary>
/// 빌드 설정을 관리하는 ScriptableObject
/// Assets/Create/Build/BuildConfig 메뉴로 생성 가능
/// </summary>
[CreateAssetMenu(fileName = "BuildConfig", menuName = "Build/BuildConfig")]
public class BuildConfig : ScriptableObject
{
    [Header("기본 설정")]
    [Tooltip("빌드 대상 플랫폼")]
    public BuildTargetType buildTargetType = BuildTargetType.ANDROID;

    [Tooltip("빌드 출력 형식")]
    public BuildOutputType buildOutputType = BuildOutputType.APK;

    [Header("Define Symbols")]
    [Tooltip("빌드 시 적용할 Define Symbols (세미콜론으로 구분)")]
    [TextArea(3, 5)]
    public string defineSymbols = "";

    [Header("빌드 옵션")]
    [Tooltip("개발 빌드 여부")]
    public bool developmentBuild = false;

    [Tooltip("스크립트 디버깅 허용")]
    public bool allowDebugging = false;

    [Tooltip("프로파일러 연결 허용")]
    public bool connectWithProfiler = false;

    [Tooltip("딥 프로파일링 지원")]
    public bool deepProfilingSupport = false;

    [Header("Android 설정")]
    [Tooltip("빌드 시 App Bundle 사용 여부 (AAB)")]
    public bool useAppBundle = false;

    [Tooltip("Minify 설정")]
    public bool minifyRelease = true;

    [Header("iOS 설정")]
    [Tooltip("Xcode 프로젝트 빌드 타입")]
    public iOSBuildType iosBuildType = iOSBuildType.DEBUG;

    [Header("씬 설정")]
    [Tooltip("빌드에 포함할 씬 목록 (비어있으면 Build Settings의 씬 사용)")]
    public SceneAsset[] buildScenes;

    // 빌드 대상 플랫폼
    public enum BuildTargetType
    {
        ANDROID,
        IOS
    }

    // 빌드 출력 형식
    public enum BuildOutputType
    {
        APK,
        AAB
    }

    // iOS 빌드 타입
    public enum iOSBuildType
    {
        DEBUG,
        RELEASE
    }

    /// <summary>
    /// 현재 설정에 맞는 BuildOptions 반환
    /// </summary>
    public BuildOptions GetBuildOptions()
    {
        var options = BuildOptions.None;

        if (developmentBuild)
        {
            options |= BuildOptions.Development;
        }

        if (allowDebugging)
        {
            options |= BuildOptions.AllowDebugging;
        }

        if (connectWithProfiler)
        {
            options |= BuildOptions.ConnectWithProfiler;
        }

        if (deepProfilingSupport)
        {
            options |= BuildOptions.EnableDeepProfilingSupport;
        }

        return options;
    }

    /// <summary>
    /// BuildTarget 반환
    /// </summary>
    public BuildTarget GetBuildTarget()
    {
        return buildTargetType == BuildTargetType.ANDROID ? BuildTarget.Android : BuildTarget.iOS;
    }

    /// <summary>
    /// BuildTargetGroup 반환
    /// </summary>
    public BuildTargetGroup GetBuildTargetGroup()
    {
        return buildTargetType == BuildTargetType.ANDROID ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
    }
}
