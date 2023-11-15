using Fixie;

namespace Parsley.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        if (environment.IsContinuousIntegration())
            configuration.Reports.Add(new GitHubReport(environment));
    }
}
