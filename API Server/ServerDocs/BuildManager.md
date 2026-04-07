# BuildManager

빌드 자동화를 실행하는 EditorWindow 스크립트입니다.

## 파일 위치

`Assets/Script/Editor/Build/BuildManager.cs`

## 사용 방법

### 1. BuildConfig 생성

1. Project 창에서 우클릭
2. `Create > Build > BuildConfig` 선택
3. 원하는 빌드 설정 구성

### 2. Build Window 열기

1. 메뉴에서 `Tools > Build > Build Window` 선택
2. EditorWindow가 열림

### 3. BuildConfig 주입

1. Build Window의 "Config 파일" 필드에 BuildConfig 에셋을 드래그앤드롭
2. 현재 설정이 미리보기로 표시됨

### 4. 빌드 실행

1. 필요시 "빠른 설정" 버튼으로 플랫폼 변경
2. "빌드 시작" 버튼 클릭
3. 확인 다이얼로그 후 빌드 진행

## MenuItem 메뉴

| 메뉴 | 설명 |
|------|------|
| `Tools/Build/Build Window` | 빌드 매니저 창 열기 |
| `Tools/Build/Open Build Folder` | 빌드 출력 폴더 열기 |

## Build Window 구성

### Build Config 영역
- BuildConfig ScriptableObject를 드래그앤드롭으로 할당

### 현재 설정 영역
- 할당된 Config의 설정 미리보기 (읽기 전용)
- "Config 수정하기" 버튼으로 Inspector에서 수정 가능

### 빠른 설정 영역
- `Android APK` - Android APK 빌드로 빠르게 전환
- `Android AAB` - Android AAB 빌드로 빠르게 전환
- `iOS` - iOS 빌드로 빠르게 전환

### 빌드 실행 영역
- 출력 경로 미리보기
- "빌드 시작" 버튼
- "빌드 폴더 열기" 버튼

## 빌드 출력

- **Android APK**: `Builds/ProductName_yyyyMMdd_HHmmss.apk`
- **Android AAB**: `Builds/ProductName_yyyyMMdd_HHmmss.aab`
- **iOS**: `Builds/ProductName_yyyyMMdd_HHmmss_iOS/` (Xcode 프로젝트)

## 참고

- [BuildConfig.md](BuildConfig.md) - 빌드 설정 ScriptableObject 문서
