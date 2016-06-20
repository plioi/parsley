namespace Build
{
    using System.IO;
    using static Shell;

    public class Commands
    {
        private string configuration = "Release";
        private string artifacts = @".\artifacts";
        private string parsley = @".\src\Parsley";
        private string tests = @".\src\Parsley.Tests";

        public void Default()
        {
            Clean();
            Restore();
            Build();
            Test();
        }

        public void Package(int buildNumber)
        {
            Default();
            Pack(buildNumber);
        }

        void Clean()
        {
            if (Directory.Exists(artifacts))
                Directory.Delete(artifacts, true);
        }

        void Restore()
            => dotnet("restore --verbosity Warning");

        void Build()
            => dotnet($"build {parsley} --configuration {configuration}");

        void Test()
            => dotnet($"test {tests} --configuration {configuration}");

        void Pack(int buildNumber)
            => dotnet($"pack {parsley} --configuration {configuration} --output {artifacts} --version-suffix={buildNumber:D4}");
    }
}