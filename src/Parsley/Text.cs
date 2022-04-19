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

    public void Advance(ref Position position, int characters)
    {
        if (characters == 0)
            return;

        int lineDelta = 0;
        int columnDelta = 0;

        var peek = Peek(characters);

        foreach (var ch in peek)
        {
            if (ch == '\n')
            {
                lineDelta++;
                columnDelta = 0 - position.Column;
            }

            columnDelta++;
        }

        input = input.Slice(peek.Length);

        position.Line += lineDelta;
        position.Column += columnDelta;
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
