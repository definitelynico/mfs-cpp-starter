using System.Diagnostics;
using System.Text.RegularExpressions;

public class FileGenerator
{
    private readonly string _projectPath;
    private readonly string _projectName;
    private readonly string _cmakeVersion;
    private readonly string _preferredGenerator;

    public FileGenerator(string projectPath, string projectName)
    {
        _projectPath = projectPath;
        _projectName = projectName;
        _cmakeVersion = GetCMakeVersion();
        _preferredGenerator = DetectPreferredGenerator();
    }

    private static bool IsDependencyAvailable(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static void CheckDependencies()
    {
        var missing = new List<string>();

        if (!IsDependencyAvailable("cmake")) missing.Add("CMake");
        if (!IsDependencyAvailable("git")) missing.Add("Git");

        if (missing.Any())
        {
            throw new ApplicationException(
                $"Missing required dependencies: {string.Join(", ", missing)}\n" +
                "Please ensure they are installed and available in your PATH.");
        }
    }

    private string GetCMakeVersion()
    {
        try
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

            if (process?.ExitCode != 0)
            {
                throw new ApplicationException("Failed to get CMake version.");
            }

            // Extract version from "cmake version X.Y.Z" and trim to X.Y
            var version = output.Split(' ').Length >= 3 ? output.Split(' ')[2] : "3.20.0";
            var match = Regex.Match(version, @"(\d+\.\d+)");
            return match.Success ? match.Groups[1].Value : "3.20";
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            throw new ApplicationException("Failed to execute CMake. Please ensure it's properly installed.");
        }
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
        GenerateClangd();
        GenerateGitIgnore();
        GenerateRunScripts();
        GenerateProjectReadme();
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
project({_projectName} LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Create executable
add_executable(${{PROJECT_NAME}})

# Define sources
target_sources(${{PROJECT_NAME}}
    PRIVATE
        src/main.cpp
)

# Set include directories directly on target
target_include_directories(${{PROJECT_NAME}}
    PRIVATE 
        ""${{CMAKE_SOURCE_DIR}}/include""
        # Add additional include directories here, for example:
        # ""${{CMAKE_SOURCE_DIR}}/include/raylib""
)

# Link dependencies
target_link_libraries(${{PROJECT_NAME}}
    PRIVATE
        # Add your library dependencies here, for example:
        # ""${{CMAKE_SOURCE_DIR}}/lib/raylib.lib""
)

# Platform-specific dependencies
if(WIN32)
    # Add your Windows-specific libraries here, for example:
    # target_link_libraries(${{PROJECT_NAME}} PRIVATE winmm)
elseif(APPLE)
    # Add your macOS-specific libraries here
elseif(UNIX AND NOT APPLE)
    # Add your Linux-specific libraries here
endif()";

        File.WriteAllText(Path.Combine(_projectPath, "CMakeLists.txt"), content);
    }

    private void GenerateCMakePresets()
    {
        string generator = _preferredGenerator.Length > 0 ? _preferredGenerator : "Ninja";
        string content = $@"{{
    ""version"": 6,
    ""configurePresets"": [
        {{
            ""name"": ""base"",
            ""hidden"": true,
            ""generator"": ""{generator}"",
            ""binaryDir"": ""${{sourceDir}}/build/${{presetName}}""
        }},
        {{
            ""name"": ""debug"",
            ""inherits"": ""base"",
            ""displayName"": ""Debug"",
            ""cacheVariables"": {{
                ""CMAKE_BUILD_TYPE"": ""Debug"",
                ""CMAKE_EXPORT_COMPILE_COMMANDS"": ""ON""
            }}
        }},
        {{
            ""name"": ""release"",
            ""inherits"": ""base"",
            ""displayName"": ""Release"",
            ""cacheVariables"": {{
                ""CMAKE_BUILD_TYPE"": ""Release""
            }}
        }}
    ],
    ""buildPresets"": [
        {{
            ""name"": ""debug"",
            ""configurePreset"": ""debug""
        }},
        {{
            ""name"": ""release"",
            ""configurePreset"": ""release""
        }}
    ]
}}";
        File.WriteAllText(Path.Combine(_projectPath, "CMakePresets.json"), content);
    }

    private void GenerateClangFormat()
    {
        string content = @"BasedOnStyle: LLVM
IndentWidth: 4
ColumnLimit: 120
BreakBeforeBraces: Allman";
        File.WriteAllText(Path.Combine(_projectPath, ".clang-format"), content);
    }

    private void GenerateClangd()
    {
        string content = @"CompileFlags:
  CompilationDatabase: build/debug";
        File.WriteAllText(Path.Combine(_projectPath, ".clangd"), content);
    }

    private void GenerateGitIgnore()
    {
        string content = @"build/
.cache/";
        File.WriteAllText(Path.Combine(_projectPath, ".gitignore"), content);
    }

    private void GenerateRunScripts()
    {
        if (OperatingSystem.IsWindows())
        {
            // Generate PowerShell script
            string psContent = $@"# If debug folder doesn't exist, run ""cmake --preset debug"" to create it
if (-not (Test-Path .\build\debug)) {{
    cmake --preset debug
}}

# Build using CMake preset
cmake --build --preset debug

# Run the executable
.\build\debug\{_projectName}.exe";
            File.WriteAllText(Path.Combine(_projectPath, "run.ps1"), psContent);
        }
        else
        {
            // Generate Shell script
            string shContent = $@"#!/bin/bash

# If debug folder doesn't exist, run ""cmake --preset debug"" to create it
if [ ! -d ""./build/debug"" ]; then
    cmake --preset debug
fi

# Build using CMake preset
cmake --build --preset debug

# Run the executable
./build/debug/{_projectName}";
            File.WriteAllText(Path.Combine(_projectPath, "run.sh"), shContent);

            // Make shell script executable
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x {Path.Combine(_projectPath, "run.sh")}",
                    UseShellExecute = false
                };
                using var process = Process.Start(startInfo);
                process?.WaitForExit();
            }
            catch
            {
                // Ignore chmod errors
            }
        }
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

    private void GenerateProjectReadme()
    {
        string content = $@"# {_projectName}

A modern C++23 project using CMake.

## Quick Start

Run the appropriate script for your platform:

```bash
# Windows
./run.ps1

# Linux/macOS
./run.sh
```

## Manual Build Commands

```bash
# Debug build (with compile_commands.json)
cmake --preset debug
cmake --build --preset debug

# Release build
cmake --preset release
cmake --build --preset release
```

## Project Structure

```
{_projectName}/
├── src/              # Source files (.cpp)
├── include/          # Header files (.hpp)
├── lib/             # External libraries
└── build/           # Build outputs
```

## Development Guide

### Adding Source Files

1. Create your .cpp files in `src/`
2. Create your .hpp files in `include/`
3. Add new source files to CMakeLists.txt:
   ```cmake
   target_sources(${{PROJECT_NAME}}
       PRIVATE
           src/your_new_file.cpp
   )
   ```

### Adding Dependencies

1. Place external libraries in `lib/`
2. Add their headers in `include/`
3. Update CMakeLists.txt:
   ```cmake
   # Add include directories
   target_include_directories(${{PROJECT_NAME}}
       PRIVATE 
           ""${{CMAKE_SOURCE_DIR}}/include/your_lib""
   )

   # Link libraries
   target_link_libraries(${{PROJECT_NAME}}
       PRIVATE
           ""${{CMAKE_SOURCE_DIR}}/lib/your_lib.lib""
   )
   ```

### IDE Support

This project includes:
- `.clangd` configuration for LSP support
- `.clang-format` for consistent code styling
- `compile_commands.json` (generated in debug builds)

### Build Outputs

- Debug build: `build/debug/`
- Release build: `build/release/`
- Compilation database: `build/debug/compile_commands.json`";

        File.WriteAllText(Path.Combine(_projectPath, "README.md"), content);
    }
}