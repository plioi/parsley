namespace Parsley;

public ref struct Text
{
    public ReadOnlySpan<char> input;

    public Text(ReadOnlySpan<char> input)
        => this.input = input;

    public readonly override string ToString()
        => input.ToString();
}

public static class TextExtensions
{
    public static ReadOnlySpan<char> Peek(this ref Text text, int characters)
        => characters >= text.input.Length
            ? text.input.Slice(0)
            : text.input.Slice(0, characters);

    public static void Advance(this ref Text text, ref Position position, int characters)
    {
        if (characters == 0)
            return;

        int lineDelta = 0;
        int columnDelta = 0;

        var peek = text.Peek(characters);

        foreach (var ch in peek)
        {
            if (ch == '\n')
            {
                lineDelta++;
                columnDelta = 0 - position.Column;
            }

            columnDelta++;
        }

        text.input = text.input.Slice(peek.Length);

        position.Line += lineDelta;
        position.Column += columnDelta;
    }

    public static bool EndOfInput(this ref Text text) => text.input.Length == 0;

    public static ReadOnlySpan<char> TakeWhile(this ref Text text, Predicate<char> test)
    {
        int i = 0;

        while (i < text.input.Length && test(text.input[i]))
            i++;

        return text.Peek(i);
    }
}
