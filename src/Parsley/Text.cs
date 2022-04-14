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

    public ReadOnlySpan<char> Peek(int characters)
        => index + characters >= input.Length
            ? input.Slice(index)
            : input.Slice(index, characters);

    public void Advance(int characters)
    {
        if (characters == 0)
            return;

        int newIndex = index + characters;
        int countNewLines = 0;

        foreach (var ch in Peek(characters))
            if (ch == '\n')
                countNewLines++;

        int newLineNumber = line + countNewLines;

        index = newIndex;
        line = newLineNumber;

        if (index > input.Length)
            index = input.Length;
    }

    public bool EndOfInput => index >= input.Length;

    public bool TryMatch(Predicate<char> test, out string value)
    {
        int i = index;

        while (i < input.Length && test(input[i]))
            i++;

        value = Peek(i - index).ToString();

        return value.Length > 0;
    }

    int Column
    {
        get
        {
            if (index == 0)
                return 1;

            int indexOfPreviousNewLine = input[..index].LastIndexOf('\n');
            return index - indexOfPreviousNewLine;
        }
    }

    public Position Position
        => new(line, Column);

    public (int index, int line) Snapshot()
        => (index, line);

    public void Restore((int index, int line) snapshot)
    {
        index = snapshot.index;
        line = snapshot.line;
    }

    public override string ToString()
        => input.Slice(index).ToString();
}
