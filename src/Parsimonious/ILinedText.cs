namespace Parsimonious
{
    public interface ILinedText : IText
    {
        bool ReadLine();
        bool EndOfLine { get; }
    }
}
