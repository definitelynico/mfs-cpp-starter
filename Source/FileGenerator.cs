using System.Diagnostics;

public class FileGenerator
{
    private readonly string _projectPath;
    private readonly string _cmakeVersion;
    private readonly string _preferredGenerator;

    public FileGenerator(string projectPath)
    {
        _projectPath = projectPath;
        _cmakeVersion = GetCMakeVersion();
        _preferredGenerator = DetectPreferredGenerator();
    }

    private string GetCMakeVersion()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmake",
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        string output = process?.StandardOutput.ReadLine() ?? "";
        process?.WaitForExit();

        // Extract version from "cmake version X.Y.Z"
        return output.Split(' ').Length >= 3 ? output.Split(' ')[2] : "3.20.0";
    }

    private string DetectPreferredGenerator()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ninja",
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        try
        {
            using var process = Process.Start(startInfo);
            process?.WaitForExit();
            return "Ninja";
        }
        catch
        {
            return ""; // Default generator
        }
    }

    public void GenerateProject()
    {
        CreateDirectories();
        GenerateMainCpp();
        GenerateCMakeLists();
        GenerateCMakePresets();
        GenerateClangFormat();
        GenerateGitIgnore();
        InitializeGit();
    }

    private void CreateDirectories()
    {
        Directory.CreateDirectory(Path.Combine(_projectPath, "src"));
        Directory.CreateDirectory(Path.Combine(_projectPath, "include"));
        Directory.CreateDirectory(Path.Combine(_projectPath, "lib"));
        Directory.CreateDirectory(Path.Combine(_projectPath, "build"));
    }

    private void GenerateMainCpp()
    {
        string content = @"#include <iostream>

int main()
{
    std::cout << ""Hello, World!"" << std::endl;
    return 0;
}";
        File.WriteAllText(Path.Combine(_projectPath, "src", "main.cpp"), content);
    }

    private void GenerateCMakeLists()
    {
        string content = $@"cmake_minimum_required(VERSION {_cmakeVersion})
project(${{PROJECT_NAME}} LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(${{PROJECT_NAME}} src/main.cpp)
target_include_directories(${{PROJECT_NAME}} PRIVATE include)
target_link_directories(${{PROJECT_NAME}} PRIVATE lib)";

        File.WriteAllText(Path.Combine(_projectPath, "CMakeLists.txt"), content);
    }

    private void GenerateCMakePresets()
    {
        string generator = _preferredGenerator.Length > 0 ? $@"""generator"": ""{_preferredGenerator}""," : "";
        string content = $@"{{
    ""version"": 3,
    ""configurePresets"": [
        {{
            ""name"": ""debug"",
            {generator}
            ""binaryDir"": ""${{sourceDir}}/build/debug"",
            ""cacheVariables"": {{
                ""CMAKE_BUILD_TYPE"": ""Debug"",
                ""CMAKE_EXPORT_COMPILE_COMMANDS"": true
            }}
        }},
        {{
            ""name"": ""release"",
            {generator}
            ""binaryDir"": ""${{sourceDir}}/build/release"",
            ""cacheVariables"": {{
                ""CMAKE_BUILD_TYPE"": ""Release"",
                ""CMAKE_EXPORT_COMPILE_COMMANDS"": true
            }}
        }}
    ]
}}";
        File.WriteAllText(Path.Combine(_projectPath, "CMakePresets.json"), content);
    }

    private void GenerateClangFormat()
    {
        string content = @"BasedOnStyle: LLVM
IndentWidth: 4
ColumnLimit: 80
BreakBeforeBraces: Allman";
        File.WriteAllText(Path.Combine(_projectPath, ".clang-format"), content);
    }

    private void GenerateGitIgnore()
    {
        string content = @"build/
.cache/";
        File.WriteAllText(Path.Combine(_projectPath, ".gitignore"), content);
    }

    private void InitializeGit()
    {
        RunCommand("git", "init");
        RunCommand("git", "add .");
        RunCommand("git", "commit -m \"init\"");
    }

    private void RunCommand(string command, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = _projectPath,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }
}