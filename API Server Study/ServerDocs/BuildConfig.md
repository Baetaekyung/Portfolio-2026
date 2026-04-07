# BuildConfig

빌드 자동화를 위한 ScriptableObject 기반 설정 시스템입니다.

## 파일 위치

- `Assets/Script/Editor/Build/BuildConfig.cs` - ScriptableObject 정의
- `Assets/Script/Editor/Build/BuildManager.cs` - 빌드 관리 에디터 스크립트

## BuildConfig 생성

1. Project 창에서 우클릭
2. `Create > Build > BuildConfig` 선택
3. 또는 `Tools > Build > Select Build Config` 메뉴 사용 (자동 생성)

## 설정 항목

### 기본 설정

| 항목 | 설명 |
|------|------|
| Build Target Type | 빌드 대상 플랫폼 (ANDROID, IOS) |
| Build Output Type | 출력 형식 (APK, AAB) |

### Define Symbols

빌드 시 적용할 스크립팅 심볼을 세미콜론(;)으로 구분하여 입력합니다.

예: `DEBUG_MODE;ENABLE_LOGS;TEST_BUILD`

### 빌드 옵션

| 항목 | 설명 |
|------|------|
| Development Build | 개발 빌드 여부 |
| Allow Debugging | 스크립트 디버깅 허용 |
| Connect With Profiler | 프로파일러 연결 허용 |
| Deep Profiling Support | 딥 프로파일링 지원 |

### Android 설정

| 항목 | 설명 |
|------|------|
| Use App Bundle | AAB 형식으로 빌드 |
| Minify Release | 릴리즈 빌드 Minify 적용 |

### iOS 설정

| 항목 | 설명 |
|------|------|
| iOS Build Type | Xcode 빌드 타입 (DEBUG, RELEASE) |

### 씬 설정

| 항목 | 설명 |
|------|------|
| Build Scenes | 빌드에 포함할 씬 목록 (비어있으면 Build Settings 사용) |

## MenuItem 메뉴

### Tools > Build

| 메뉴 | 설명 |
|------|------|
| Set Android Build - APK | Android APK 빌드로 설정 |
| Set Android Build - AAB | Android AAB 빌드로 설정 |
| Set iOS Build - APK | iOS 빌드로 설정 |
| Set iOS Build - AAB | iOS 빌드로 설정 |
| Start Build | 현재 설정으로 빌드 실행 |
| Open Build Folder | 빌드 출력 폴더 열기 |
| Select Build Config | BuildConfig 에셋 선택 |

## 빌드 출력 경로

빌드 결과물은 프로젝트 루트(Assets 상위 폴더)의 `Builds` 폴더에 저장됩니다.

```
ProjectRoot/
├── Assets/
├── Builds/           <- 빌드 출력 폴더
│   ├── ProductName_20240115_143022.apk
│   ├── ProductName_20240115_150000.aab
│   └── ProductName_20240115_160000_iOS/
└── ...
```

## 사용 예시

### 기본 빌드 흐름

1. `Tools > Build > Select Build Config`로 설정 에셋 선택
2. Inspector에서 빌드 옵션 설정
3. `Tools > Build > Set Android Build - APK` 또는 원하는 플랫폼 선택
4. `Tools > Build > Start Build`로 빌드 실행

### 커맨드라인 빌드 (CI/CD)

```csharp
// 커맨드라인에서 호출 가능
BuildManager.SetAndroidBuildAPK();
BuildManager.StartBuild();
```
