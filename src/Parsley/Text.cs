namespace Parsley;

public ref struct Text
{
    ReadOnlySpan<char> input;

    public Text(ReadOnlySpan<char> input)
        => this.input = input;

    public readonly ReadOnlySpan<char> Peek(int characters)
        => characters >= input.Length
            ? input.Slice(0)
            : input.Slice(0, characters);

    public (int lineDelta, int columnDelta) Advance(Position start, int characters)
    {
        if (characters == 0)
            return (0, 0);

        int lineDelta = 0;
        int columnDelta = 0;

        foreach (var ch in Peek(characters))
        {
            if (ch == '\n')
            {
                lineDelta++;
                columnDelta = 0 - start.Column;
            }

            columnDelta++;
        }

        input = characters < input.Length
            ? input.Slice(characters)
            : ReadOnlySpan<char>.Empty;

        return (lineDelta, columnDelta);
    }

    public readonly bool EndOfInput => input.Length == 0;

    public readonly ReadOnlySpan<char> TakeWhile(Predicate<char> test)
    {
        int i = 0;

        while (i < input.Length && test(input[i]))
            i++;

        return Peek(i);
    }

    public readonly override string ToString()
        => input.ToString();
}
