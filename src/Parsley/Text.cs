namespace Parsley;

public ref struct Text
{
    int index;
    readonly ReadOnlySpan<char> input;
    int line;

    public Text(ReadOnlySpan<char> input)
        : this(input, 0, 1) { }

    Text(ReadOnlySpan<char> input, int index, int line)
    {
        this.input = input;
        this.index = index;

        if (index > input.Length)
            this.index = input.Length;

        this.line = line;
    }

    public readonly ReadOnlySpan<char> Peek(int characters)
        => index + characters >= input.Length
            ? input.Slice(index)
            : input.Slice(index, characters);

    public (int lineDelta, int columnDelta) Advance(Position start, int characters)
    {
        if (characters == 0)
            return (0, 0);

        int originalColumn = start.Column;
        int columnDelta = 0;
        int newIndex = index + characters;
        int countNewLines = 0;

        foreach (var ch in Peek(characters))
        {
            if (ch == '\n')
            {
                countNewLines++;
                columnDelta = 0 - originalColumn;
            }

            columnDelta++;
        }

        int newLineNumber = line + countNewLines;

        index = newIndex;
        line = newLineNumber;

        if (index > input.Length)
            index = input.Length;

        return (countNewLines, columnDelta);
    }

    public readonly bool EndOfInput => index >= input.Length;

    public readonly ReadOnlySpan<char> TakeWhile(Predicate<char> test)
    {
        int i = index;

        while (i < input.Length && test(input[i]))
            i++;

        return Peek(i - index);
    }

    public readonly override string ToString()
        => input.Slice(index).ToString();
}
