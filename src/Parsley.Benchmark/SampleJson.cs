using System.Text.Json;

namespace Parsley.Benchmark;

public class SampleJson
{
    static readonly Random random = new();

    static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        MaxDepth = 1000
    };

    readonly string scenario;
    public string Json { get; }

    public SampleJson(string scenario, int length, int depth)
    {
        this.scenario = scenario;

        var list = new List<object>(length);

        for (int i = 1; i <= length; i++)
            list.Add(BuildObject(depth));

        Json = JsonSerializer.Serialize(list, serializerOptions);
    }

    public override string ToString() => scenario;

    static string RandomString() => Guid.NewGuid().ToString();
    
    static object BuildObject(int depth)
        => depth == 1
            ? new Leaf()
            : new Branch { Inner = BuildObject(depth - 1) };

    class Branch
    {
        public object? Inner { get; set; }
    }

    class Leaf
    {
        public string First { get; init; } = RandomString();
        public string Second { get; init; } = RandomString();
        public string Third { get; init; } = RandomString();
        public bool Boolean { get; init; } = random.NextDouble() > 0.5;
        public int Integer { get; init; } = random.Next(0, int.MaxValue);
        public object? Null { get; init; }
    }
}
