# DTOGenerator.md

## 개요
C# Interface 정의를 기반으로 Server용 record와 Unity용 class를 자동 생성하는 도구.

## 경로
`Project_Q_Tools/DTOGenerator/DTOGenerator.cs`

## 사용법

### 1. 빌드
```powershell
cd c:\Project_Q\Project_Q_Tools\DTOGenerator
dotnet build
```

### 2. 실행
```powershell
# 기본 경로 사용
dotnet run

# 커스텀 경로 지정
dotnet run [ServerOutputPath] [ClientOutputPath] [AssemblyPath]
```

## Interface 정의 규칙
- 파일 위치: `Q_Server/DTOs/Definitions/`
- 네이밍: `I{Name}DTO` 형식 (예: `ILoginRequestDTO`)

```csharp
public interface ILoginRequestDTO
{
    string Username { get; }
    string Password { get; }
}
```

## 생성 결과

### Server (record)
```csharp
public record LoginRequestDTO(string Username, string Password);
```

### Unity Client (class)
```csharp
[Serializable]
public class LoginRequestDTO
{
    public string username;  // camelCase
    public string password;
}
```

## 출력 경로
- **Server**: `Q_Server/DTOs/Generated/`
- **Client**: `Project_Q_Unity/Assets/Scripts/DTOs/Generated/`

## 관련 파일
- [IDTODefinitions.cs](file:///c:/Project_Q/Project_Q_Server/Q_Server/DTOs/Definitions/IDTODefinitions.cs)
