namespace Build
{
    using System;
    using System.Linq;
    using System.Reflection;
    using static System.Console;

    public static class Program
    {
        private const int Success = 0;
        private const int Failure = 1;

        public static int Main(string[] args)
        {
            try
            {
                var command = args.FirstOrDefault()?.ToLower() ?? "default";

                var method = CommandMethods
                    .SingleOrDefault(m => string.Equals(m.Name, command, StringComparison.CurrentCultureIgnoreCase));

                if (method == null)
                {
                    if (command == "help")
                        DefaultHelp();
                    else
                        throw new Exception("Unknown command: " + command);

                    return Failure;
                }

                method.Invoke(new Commands(), Parameters(method, args.Skip(1).ToArray()));

                using (Foreground.Green)
                {
                    WriteLine();
                    WriteLine("Build Succeeded!");
                }

                return Success;
            }
            catch (Exception exception)
            {
                using (Foreground.DarkRed)
                {
                    WriteLine();
                    WriteLine(exception.Message);
                    WriteLine();
                    WriteLine("Build Failed!");
                }

                return Failure;
            }
        }

        private static object[] Parameters(MethodInfo method, string[] args)
        {
            var declaredParameters = method.GetParameters();

            if (declaredParameters.Length != args.Length)
                throw new Exception("Parameter count mismatch.");

            if (declaredParameters.Length == 0)
                return null;

            return args
                .Take(declaredParameters.Length)
                .Select((str, i) => Convert.ChangeType(str, declaredParameters[i].ParameterType))
                .ToArray();
        }

        private static MethodInfo[] CommandMethods
            => typeof(Commands)
                .GetMethods()
                .Where(IsCommand)
                .ToArray();

        static void DefaultHelp()
        {
            WriteLine("Commands:");

            foreach (var method in CommandMethods)
            {
                var arguments = String.Join(" ", method.GetParameters().Select(p => "$" + p.Name));

                WriteLine($"    build {method.Name.ToLower()} {arguments}");
            }
        }

        static bool IsCommand(MethodInfo method)
            => method.IsPublic && !method.IsStatic && method.DeclaringType != typeof(object);
    }
}