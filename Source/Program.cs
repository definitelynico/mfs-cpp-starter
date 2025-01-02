using System.Text.RegularExpressions;

namespace MfsCppStarter;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  mfs-cpp-starter init [project-name]     # Initialize in current directory");
            Console.WriteLine("  mfs-cpp-starter <project-path> [name]   # Create in new directory");
            return;
        }

        string command = args[0];
        string projectPath;
        string projectName;

        if (command == "init")
        {
            projectPath = Directory.GetCurrentDirectory();
            projectName = args.Length > 1 ? args[1] : new DirectoryInfo(projectPath).Name;
        }
        else
        {
            projectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), command));
            projectName = args.Length > 1 ? args[1] : Path.GetFileName(projectPath);
        }

        if (!IsValidProjectName(projectName))
        {
            Console.WriteLine("Error: Project name must be alphanumeric with hyphens or underscores only");
            return;
        }

        try
        {
            bool isInit = command == "init";
            if (Directory.Exists(projectPath))
            {
                // For init command, check if directory only contains our executable
                if (isInit)
                {
                    var currentExe = Path.GetFileName(Environment.ProcessPath ?? "");
                    var files = Directory.GetFiles(projectPath);
                    var dirs = Directory.GetDirectories(projectPath);
                    bool onlyHasExe = files.Length <= 1 &&
                                    dirs.Length == 0 &&
                                    (files.Length == 0 ||
                                     Path.GetFileName(files[0]).Equals(currentExe, StringComparison.OrdinalIgnoreCase));

                    if (!onlyHasExe)
                    {
                        Console.WriteLine("Error: Current directory contains other files");
                        return;
                    }
                }
                else if (Directory.EnumerateFileSystemEntries(projectPath).Any())
                {
                    Console.WriteLine($"Error: Directory '{projectPath}' exists and is not empty");
                    return;
                }
            }
            else if (!isInit)
            {
                Directory.CreateDirectory(projectPath);
            }

            var generator = new FileGenerator(projectPath, projectName);
            generator.GenerateProject();
            Console.WriteLine($"Successfully created C++ project '{projectName}'");
            Console.WriteLine("\nTo build your project:");
            Console.WriteLine($"cd {projectName}");
            Console.WriteLine("cmake --preset debug");
            Console.WriteLine("cmake --build build/debug");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating project: {ex.Message}");
            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, true);
            }
        }
    }

    private static bool IsValidProjectName(string name)
    {
        // Split path into segments and check the last segment (actual project name)
        string[] segments = name.Split('/', '\\');
        string projectName = segments[^1];
        return Regex.IsMatch(projectName, "^[a-zA-Z0-9_-]+$");
    }
}