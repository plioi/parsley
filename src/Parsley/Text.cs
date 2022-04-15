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

    public readonly bool EndOfInput => index >= input.Length;

    public readonly ReadOnlySpan<char> TakeWhile(Predicate<char> test)
    {
        int i = index;

        while (i < input.Length && test(input[i]))
            i++;

        return Peek(i - index);
    }

    readonly int Column
    {
        get
        {
            if (index == 0)
                return 1;

            int indexOfPreviousNewLine = input[..index].LastIndexOf('\n');
            return index - indexOfPreviousNewLine;
        }
    }

    public readonly Position Position
        => new(line, Column);

    public readonly override string ToString()
        => input.Slice(index).ToString();
}
