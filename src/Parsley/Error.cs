using System.Text;

namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(string expectation)
        => Expectation = expectation;

    public Error(IReadOnlyList<string> expectations)
        => Expectation = CompoundExpectation(expectations);

    public T Value => throw new MemberAccessException("Cannot access Value for an Error reply.");
    public bool Success => false;
    public string Expectation { get; }

    static string CompoundExpectation(IReadOnlyList<string> expectations)
    {
        if (expectations.Count == 1)
            return expectations[0];

        if (expectations.Count == 2)
            return $"({expectations[0]} or {expectations[1]})";

        var combined = new StringBuilder();

        combined.Append('(');

        for (var i = 0; i < expectations.Count; i++)
        {
            if (i == 0)
            {
                combined.Append(expectations[i]);
            }
            else
            {
                var separator = i == expectations.Count - 1
                    ? ", or "
                    : ", ";

                combined.Append(separator + expectations[i]);
            }
        }

        combined.Append(')');

        return combined.ToString();
    }
}
