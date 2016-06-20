namespace Build
{
    using System;
    using System.Diagnostics;

    public static class Shell
    {
        public static void run(string commandName, string arguments = "")
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = commandName,
                    Arguments = arguments
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Error executing command: {commandName} {arguments}");
        }

        public static void dotnet(string command)
        {
            using (Foreground.Cyan)
                Console.WriteLine($"Executing dotnet {command}");

            run("dotnet", command);
        }
    }
}