namespace Parsley;

public static class Characters
{
    public static readonly Func<char, bool> IsWhiteSpace = char.IsWhiteSpace;
    public static readonly Func<char, bool> IsLower = char.IsLower;
    public static readonly Func<char, bool> IsUpper = char.IsUpper;
    public static readonly Func<char, bool> IsLetter = char.IsLetter;
    public static readonly Func<char, bool> IsDigit = char.IsDigit;
    public static readonly Func<char, bool> IsLetterOrDigit = char.IsLetterOrDigit;
    public static readonly Func<char, bool> IsControl = char.IsControl;
}
