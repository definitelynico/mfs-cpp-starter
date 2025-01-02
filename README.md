# Modern C++ Project Generator

A simple CLI tool that sets up a modern C++ project with CMake, similar to how `cargo new` works for Rust projects.

## Why

I get confused when opening an IDE - simple as. I just want to be able to create a C++ project quickly and get going, but I'm also terrible at remembering all the commands, arguments and flags to create things like .clang-format, compile-commands.json and others in order to have a pleasant experience in [your favorite terminal text editor].

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
dotnet publish -c Release -r win-x64
```

The compiled executable will be in `bin/Release/net9.0/win-x64/publish/`.

## Usage

There are two ways to create a new project:

### 1. Initialize in Current Directory

```bash
mfs-cpp-starter init [project-name]
```
If project name is not provided, the current directory name will be used.

### 2. Create in New Directory

```bash
mfs-cpp-starter <project-path> [project-name]
```
If project name is not provided, the directory name will be used.

### Examples

```bash
# Initialize in current directory
mfs-cpp-starter init my-project

# Create new project in a new directory
mfs-cpp-starter path/to/new-project

# Create new project with custom name
mfs-cpp-starter path/to/directory custom-name
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
# If using init in current directory:
cmake --preset debug
cmake --build build/debug

# If created in new directory:
cd <project-directory>
cmake --preset debug
cmake --build build/debug

# For release builds:
cmake --preset release
cmake --build build/release
```

## Requirements

- .NET 6.0 or later
- CMake 3.20 or later
- A C++ compiler (GCC, Clang, or MSVC)
- Git
- Ninja (optional, but recommended)
