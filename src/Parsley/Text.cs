namespace Parsley;

public class Text
{
    int index;
    readonly string input;
    int line;

    public Text(string input)
        : this(input, 0, 1) { }

    Text(string input, int index, int line)
    {
        this.input = input;
        this.index = index;

        if (index > input.Length)
            this.index = input.Length;

        this.line = line;
    }

    public string Peek(int characters)
        => index + characters >= input.Length
            ? input.Substring(index)
            : input.Substring(index, characters);

    public void Advance(int characters)
    {
        if (characters == 0)
            return;

        int newIndex = index + characters;
        int newLineNumber = line + Peek(characters).Cast<char>().Count(ch => ch == '\n');

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

        value = Peek(i - index);

        return value.Length > 0;
    }

    int Column
    {
        get
        {
            if (index == 0)
                return 1;

            int indexOfPreviousNewLine = input.LastIndexOf('\n', index - 1);
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
        => input.Substring(index);
}
