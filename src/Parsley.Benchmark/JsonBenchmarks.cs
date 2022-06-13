using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Parsley.Benchmark;

[MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class JsonBenchmarks
{
    public static IEnumerable<SampleJson> Scenarios()
    {
        yield return new SampleJson("Typical", length: 10, depth: 3);
        yield return new SampleJson("Repetition", length: 256, depth: 1);
        yield return new SampleJson("Recursion", length: 1, depth: 256);
    }

    [Benchmark(Description = "System.Text.Json", Baseline = true)]
    [ArgumentsSource(nameof(Scenarios))]
    public object? SystemTextJson(SampleJson sample)
        => System.Text.Json.JsonSerializer.Deserialize<object?>(sample.Json, systemJsonOptions);

    [Benchmark(Description = "Newtonsoft.Json")]
    [ArgumentsSource(nameof(Scenarios))]
    public object? NewtonsoftJson(SampleJson sample)
        => Newtonsoft.Json.JsonConvert.DeserializeObject<object?>(sample.Json, newtonsoftJsonOptions);

    [Benchmark]
    [ArgumentsSource(nameof(Scenarios))]
    public object? Parsley(SampleJson sample)
        => ParsleyJsonParser.Parse(sample.Json);

    [Benchmark]
    [ArgumentsSource(nameof(Scenarios))]
    public object? Pidgin(SampleJson sample)
        => PidginJsonParser.Parse(sample.Json);

    static readonly System.Text.Json.JsonSerializerOptions systemJsonOptions = new()
    {
        MaxDepth = 1000
    };

    static readonly Newtonsoft.Json.JsonSerializerSettings newtonsoftJsonOptions = new()
    {
        MaxDepth = 1000
    };
}
