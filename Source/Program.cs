using System.Text.RegularExpressions;

namespace MfsCppStarter;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: mfs-cpp-starter <project-name>");
            return;
        }

        string projectName = args[0];
        if (!IsValidProjectName(projectName))
        {
            Console.WriteLine("Error: Project name must be alphanumeric with hyphens or underscores only");
            return;
        }

        string projectPath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
        if (Directory.Exists(projectPath))
        {
            Console.WriteLine($"Error: Directory '{projectPath}' already exists");
            return;
        }

        try
        {
            Directory.CreateDirectory(projectPath);
            var generator = new FileGenerator(projectPath);
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
        return Regex.IsMatch(name, "^[a-zA-Z0-9_-]+$");
    }
}