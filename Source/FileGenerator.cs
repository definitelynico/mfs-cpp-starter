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
        GenerateExampleHeaders();
        GenerateCMakeLists();
        GenerateCMakePresets();
        GenerateClangFormat();
        GenerateClangd();
        GenerateGitIgnore();
        GenerateRunScripts();
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
        string content = @"#include ""animal.hpp""
#include <memory>
#include <vector>

int main()
{
    std::vector<std::unique_ptr<Animal>> animals;
    
    animals.push_back(std::make_unique<Dog>());
    animals.push_back(std::make_unique<Cat>());
    animals.push_back(std::make_unique<Rat>());

    for (const auto& animal : animals)
    {
        animal->speak();
    }

    return 0;
}";
        File.WriteAllText(Path.Combine(_projectPath, "src", "main.cpp"), content);
    }

    private void GenerateExampleHeaders()
    {
        string animalContent = @"#ifndef ANIMAL_HPP
#define ANIMAL_HPP

#include <iostream>

class Animal
{
public:
    virtual ~Animal() = default;
    virtual void speak() const = 0;
};

class Dog : public Animal
{
public:
    void speak() const override
    {
        std::cout << ""Woof!"" << std::endl;
    }
};

class Cat : public Animal
{
public:
    void speak() const override
    {
        std::cout << ""Meow!"" << std::endl;
    }
};

class Rat : public Animal
{
public:
    void speak() const override
    {
        std::cout << ""Squeak!"" << std::endl;
    }
};

#endif // ANIMAL_HPP";
        File.WriteAllText(Path.Combine(_projectPath, "include", "animal.hpp"), animalContent);
    }

    private void GenerateCMakeLists()
    {
        string content = $@"cmake_minimum_required(VERSION {_cmakeVersion})
project({_projectName} LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Define source and header files
set(SOURCE_FILES
    src/main.cpp
)

set(HEADER_FILES
    include/animal.hpp
)

# Define library paths list
set(LIB_PATHS
    # Add your library paths here, for example:
    # ""${{CMAKE_SOURCE_DIR}}/lib/yourlib.lib""
)

# Platform-specific libraries
if(WIN32)
    # Add your Windows-specific libraries here, for example:
    # list(APPEND LIB_PATHS winmm)
elseif(APPLE)
    # Add your macOS-specific libraries here
elseif(UNIX AND NOT APPLE)
    # Add your Linux-specific libraries here
endif()

# Create executable
add_executable(${{PROJECT_NAME}} ${{SOURCE_FILES}} ${{HEADER_FILES}})

# Set include directories directly on target
target_include_directories(${{PROJECT_NAME}} PRIVATE 
    ""${{CMAKE_SOURCE_DIR}}/include""
)

# Link libraries
target_link_directories(${{PROJECT_NAME}} PRIVATE ${{LIB_PATHS}})";

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
ColumnLimit: 80
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
}