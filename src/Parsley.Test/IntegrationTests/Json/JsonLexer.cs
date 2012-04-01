namespace Parsley.IntegrationTests.Json
{
    public class JsonLexer : Lexer
    {
        public JsonLexer()
            : base(Whitespace,
                   @null, @true, @false,
                   Comma, OpenArray, CloseArray, OpenDictionary, CloseDictionary, Colon,
                   Number, Quotation) { }

        private static readonly TokenKind Whitespace = new Pattern("whitespace", @"\s+", skippable: true);
        
        public static readonly Keyword @null = new Keyword("null");
        public static readonly Keyword @true = new Keyword("true");
        public static readonly Keyword @false = new Keyword("false");
        public static readonly Operator Comma = new Operator(",");
        public static readonly Operator OpenArray = new Operator("[");
        public static readonly Operator CloseArray = new Operator("]");
        public static readonly Operator OpenDictionary = new Operator("{");
        public static readonly Operator CloseDictionary = new Operator("}");
        public static readonly Operator Colon = new Operator(":");

        public static readonly TokenKind Quotation = new Pattern("string", @"
            # Open quote:
            ""

            # Zero or more content characters:
            (
                      [^""\\]*             # Zero or more non-quote, non-slash characters.
                |     \\ [""\\bfnrt\/]     # One of: slash-quote   \\   \b   \f   \n   \r   \t   \/
                |     \\ u [0-9a-fA-F]{4}  # \u folowed by four hex digits
            )*

            # Close quote:
            ""
        ");

        public static readonly TokenKind Number = new Pattern("number", @"
            # Look-ahead to confirm the whole-number part is either 0 or starts with 1-9:
            (?=
                0(?!\d)  |  [1-9]
            )
    
            # Whole number part:
            \d+

            # Optional fractional part:
            (\.\d+)?

            # Optional exponent
            (
                [eE]
                [+-]?
                \d+
            )?
        ");
    }
}