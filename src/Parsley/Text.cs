namespace Parsley;

public class Text
{
    readonly int index;
    readonly string input;
    readonly int line;

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

    public Text Advance(int characters)
    {
        if (characters == 0)
            return this;

        int newIndex = index + characters;
        int newLineNumber = line + Peek(characters).Cast<char>().Count(ch => ch == '\n');
            
        return new Text(input, newIndex, newLineNumber);
    }

    public bool EndOfInput => index >= input.Length;

    public MatchResult Match(TokenRegex regex) => regex.Match(input, index);

    public MatchResult Match(Predicate<char> test)
    {
        int i = index;

        while (i < input.Length && test(input[i]))
            i++;

        var value = Peek(i - index);

        if (value.Length > 0)
            return MatchResult.Succeed(value);

        return MatchResult.Fail;
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

    public override string ToString()
        => input.Substring(index);
}
