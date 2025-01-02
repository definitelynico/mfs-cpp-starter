# Modern C++ Project Generator

A simple CLI tool that sets up a modern C++ project with CMake, similar to how `cargo new` works for Rust projects.

## Features

- Creates a basic C++ project structure with src/, include/, and lib/ directories
- Generates CMake configuration with modern defaults
- Automatically detects and uses CMake version and preferred generator (Ninja if available)
- Includes debug and release build presets
- Sets up clangd support via compile_commands.json
- Configures .clang-format for consistent code style
- Initializes git repository with sensible .gitignore

## Installation

```bash
git clone https://github.com/definitelynico/mfs-cpp-starter.git
cd mfs-cpp-starter
dotnet build
```

## Publishing Native Executables

The tool can be compiled to native code for better performance and standalone distribution:

```bash
# For Windows
dotnet publish -c Release -r win-x64

# For Linux
dotnet publish -c Release -r linux-x64

# For macOS
dotnet publish -c Release -r osx-x64
```

The compiled executables will be in `bin/Release/net9.0/[runtime]/publish/` and:
- Have faster startup time
- Run without .NET runtime dependency
- Are optimized for size and speed
- Are specific to the target platform

## Usage

```bash
# Create a new project using directory name as project name
dotnet run test/my-project

# Create a new project with custom project name
dotnet run test/my-project CustomProjectName
```

## Project Structure

```
my-project/
├── src/
│   └── main.cpp
├── include/
├── lib/
├── build/
├── CMakeLists.txt
├── CMakePresets.json
├── .clang-format
└── .gitignore
```

## Building Generated Projects

After creating a project:

```bash
cd my-project
cmake --preset debug    # Configure debug build
cmake --build build/debug

# Or for release build
cmake --preset release
cmake --build build/release
```

## Requirements

- .NET 6.0 or later
- CMake 3.20 or later
- A C++ compiler (GCC, Clang, or MSVC)
- Git
- Ninja (optional, but recommended)
