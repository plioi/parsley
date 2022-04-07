namespace Parsley.Tests;

public class CharLexer : Lexer
{
    public static readonly TokenKind Character = new Pattern("Character", @".");
    public CharLexer()
        : base(Character) { }
}
