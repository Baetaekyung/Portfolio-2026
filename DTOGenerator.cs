using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DTOGenerator
{
    /// <summary>
    /// DTO 자동 생성 도구
    /// Interface 소스 파일을 파싱하여 Server용 record와 Unity용 class 생성
    /// </summary>
    public class DTOGenerator
    {
        private readonly string _serverOutputPath;
        private readonly string _clientOutputPath;

        public DTOGenerator(string serverOutputPath, string clientOutputPath)
        {
            _serverOutputPath = serverOutputPath;
            _clientOutputPath = clientOutputPath;
        }

        /// <summary>
        /// 인터페이스 정의 파일들을 파싱하여 DTO 생성
        /// </summary>
        public void GenerateFromSourceFiles(string definitionsPath)
        {
            if (!Directory.Exists(definitionsPath))
            {
                Console.WriteLine($"정의 폴더를 찾을 수 없습니다: {definitionsPath}");
                return;
            }

            var files = Directory.GetFiles(definitionsPath, "*.cs");
            Console.WriteLine($"발견된 정의 파일: {files.Length}개");

            var totalGenerated = 0;
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                totalGenerated += ParseAndGenerateFromSource(content);
            }

            if (totalGenerated == 0)
            {
                Console.WriteLine("생성된 DTO가 없습니다. 인터페이스 정의를 확인하세요.");
            }
            else
            {
                Console.WriteLine($"\n총 {totalGenerated}개의 DTO 생성 완료!");
            }
        }

        /// <summary>
        /// 소스 파일에서 인터페이스를 파싱하여 DTO 생성
        /// </summary>
        private int ParseAndGenerateFromSource(string sourceContent)
        {
            var count = 0;
            
            // 라인별로 파싱하여 인터페이스 찾기
            var lines = sourceContent.Split('\n');
            var currentInterface = "";
            var properties = new List<(string Type, string Name)>();
            var insideInterface = false;
            var braceCount = 0;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // 인터페이스 시작 감지
                if (trimmedLine.StartsWith("public interface I") && trimmedLine.Contains("DTO"))
                {
                    var match = Regex.Match(trimmedLine, @"public\s+interface\s+(I\w+DTO)");
                    if (match.Success)
                    {
                        currentInterface = match.Groups[1].Value;
                        insideInterface = true;
                        braceCount = 0;
                        properties.Clear();
                    }
                }

                if (insideInterface)
                {
                    // 중괄호 카운트
                    braceCount += trimmedLine.Count(c => c == '{');
                    braceCount -= trimmedLine.Count(c => c == '}');

                    // 속성 파싱
                    var propMatch = Regex.Match(trimmedLine, @"^(\w+\??)\s+(\w+)\s*\{\s*get\s*;\s*\}");
                    if (propMatch.Success)
                    {
                        var typeName = propMatch.Groups[1].Value;
                        var propName = propMatch.Groups[2].Value;
                        properties.Add((typeName, propName));
                    }

                    // 인터페이스 종료 감지
                    if (braceCount == 0 && trimmedLine.Contains("}"))
                    {
                        if (properties.Count > 0)
                        {
                            var dtoName = currentInterface.Substring(1); // 'I' 제거
                            GenerateServerRecord(dtoName, properties);
                            GenerateClientClass(dtoName, properties);
                            count++;
                        }
                        insideInterface = false;
                        currentInterface = "";
                        properties.Clear();
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Server용 record 생성
        /// </summary>
        private void GenerateServerRecord(string dtoName, List<(string Type, string Name)> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// 자동 생성된 파일 - 수정하지 마세요");
            sb.AppendLine($"// Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("namespace Q_Server.DTOs.Generated");
            sb.AppendLine("{");
            
            // record 정의
            sb.Append($"    public record {dtoName}(");
            
            var paramList = new List<string>();
            foreach (var (type, name) in properties)
            {
                // nullable annotation 제거
                var cleanType = RemoveNullableAnnotation(type);
                paramList.Add($"{cleanType} {name}");
            }
            sb.Append(string.Join(", ", paramList));
            sb.AppendLine(");");
            
            sb.AppendLine("}");

            // 파일 저장
            var outputPath = Path.Combine(_serverOutputPath, $"{dtoName}.cs");
            Directory.CreateDirectory(_serverOutputPath);
            File.WriteAllText(outputPath, sb.ToString());
            Console.WriteLine($"  [Server] {dtoName}.cs 생성됨");
        }

        /// <summary>
        /// Unity Client용 class 생성
        /// </summary>
        private void GenerateClientClass(string dtoName, List<(string Type, string Name)> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// 자동 생성된 파일 - 수정하지 마세요");
            sb.AppendLine($"// Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("namespace ProjectQ.DTOs");
            sb.AppendLine("{");
            sb.AppendLine("    [Serializable]");
            sb.AppendLine($"    public class {dtoName}");
            sb.AppendLine("    {");
            
            // 필드 정의 (Unity는 camelCase 필드 사용)
            foreach (var (type, name) in properties)
            {
                // nullable annotation 제거
                var cleanType = RemoveNullableAnnotation(type);
                var fieldName = ToCamelCase(name);
                sb.AppendLine($"        public {cleanType} {fieldName};");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 파일 저장
            var outputPath = Path.Combine(_clientOutputPath, $"{dtoName}.cs");
            Directory.CreateDirectory(_clientOutputPath);
            File.WriteAllText(outputPath, sb.ToString());
            Console.WriteLine($"  [Client] {dtoName}.cs 생성됨");
        }

        /// <summary>
        /// PascalCase를 camelCase로 변환
        /// </summary>
        private string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        /// <summary>
        /// Nullable annotation (?) 제거
        /// </summary>
        private string RemoveNullableAnnotation(string type)
        {
            if (string.IsNullOrEmpty(type)) return type;
            return type.TrimEnd('?');
        }
    }

    /// <summary>
    /// 콘솔 앱 진입점
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== DTO Generator ===");
            Console.WriteLine();

            // 기본 경로 설정 (Project_Q 루트 기준)
            var basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
            
            var definitionsPath = Path.Combine(basePath, "Project_Q_Server", "Q_Server", "DTOs", "Definitions");
            var serverOutput = Path.Combine(basePath, "Project_Q_Server", "Q_Server", "DTOs", "Generated");
            var clientOutput = Path.Combine(basePath, "Project_Q_Unity", "Assets", "Scripts", "DTOs", "Generated");

            // 명령줄 인자로 경로 오버라이드
            if (args.Length > 0) definitionsPath = args[0];
            if (args.Length > 1) serverOutput = args[1];
            if (args.Length > 2) clientOutput = args[2];

            Console.WriteLine($"정의 파일 경로: {definitionsPath}");
            Console.WriteLine($"Server 출력 경로: {serverOutput}");
            Console.WriteLine($"Client 출력 경로: {clientOutput}");
            Console.WriteLine();

            var generator = new DTOGenerator(serverOutput, clientOutput);
            generator.GenerateFromSourceFiles(definitionsPath);
        }
    }
}
