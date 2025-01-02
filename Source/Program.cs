using System.Text.RegularExpressions;

namespace MfsCppStarter;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: mfs-cpp-starter <project-path> [project-name]");
            return;
        }

        string projectPath = args[0];
        string projectName = args.Length > 1 ? args[1] : Path.GetFileName(projectPath);

        if (!IsValidProjectName(projectName))
        {
            Console.WriteLine("Error: Project name must be alphanumeric with hyphens or underscores only");
            return;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), projectPath);
        if (Directory.Exists(fullPath))
        {
            Console.WriteLine($"Error: Directory '{fullPath}' already exists");
            return;
        }

        try
        {
            Directory.CreateDirectory(fullPath);
            var generator = new FileGenerator(fullPath, projectName);
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
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
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