Introduction
============

Parsley is a monadic parser combinator library inspired by Haskell's Parsec (http://www.haskell.org/haskellwiki/Parsec) and F#'s FParsec (http://www.quanttec.com/fparsec/).  It can parse context-sensitive, infinite look-ahead grammars but it performs best on predictive (LL[1]) grammars.

Unlike Parsec/FParsec, Parsley provides separate lexer/parser phases.  Lexers are created with an ordered series of regex patterns, and parser grammars are expressed in terms of the tokens produced by the lexer.

Lexer Phase (Tokenization)
==========================

Strings being parsed are represented with a Text instance, which tracks the original string as well as the current parsing position:

        var text = new Text("some input to parse");

The lexing phase is performed by the Lexer class, which is initialized with an ordered series of regex patterns called TokenKinds.  TokenKinds can be skippable, if you want them to be recognized but discarded:

        var text = new Text("1 2 3 a b c");
        var lexer = new Lexer(text, new TokenKind("letter", @"[a-z]"),
                                    new TokenKind("number", @"[0-9]+"),
                                    new TokenKind("whitespace", @"\s+", skippable: true);

        Token[] tokens = lexer.ToArray();

Above, the array 'tokens' will contain 6 Token objects. Each Token contains the literal ("1", "a", etc), the TokenKind that matched it, and the Position (line/column number) where the token was found.

Parser Functions
================

A Parser of thingies is any function that consumes a Lexer and produces a parsed-thingy:

        public delegate Reply<T> Parser<out T>(Lexer tokens);

A Reply<T> describes whether or not the parser succeeded, the parsed-thingy (on success), a possibly-empty error message list, and a reference to a Lexer representing the remaining unparsed tokens.

Grammars
========

Grammars should inherit from Grammar to take advantage of several Parser primitives.  Grammars should define each grammar rule as a property returning some Parser<T> type.  Grammar rule bodies usually return LINQ queries, which allow you to glue together other grammar rules in sequence:

        private class CsvGrammar : Grammar
        {
            public static Parser<string[][]> CsvFile
            {
                get
                {
                    return from lines in ZeroOrMore(Line)
                           from eoi in EndOfInput
                           select lines.ToArray();
                }
            }

            private static Parser<string[]> Line
            {
                get
                {
                    return from values in OneOrMore(Value, Comma)
                           from eol in EndOfLine
                           select values.Select(token => token.Literal).ToArray();
                }
            }

            private static Parser<Token> EndOfLine
            {
                get { return Choice(Token(CsvLexer.EndOfLine), Token(Lexer.EndOfInput)); }
            }

            private static Parser<Token> Comma
            {
                get { return Token(","); }
            }

            private static Parser<Token> Value
            {
                get { return Token(CsvLexer.Value); }
            }
        }

Finally, we can put all these pieces together to parse some text:

        const string input = "abc,123,DEF\r\nghi,456,JKL\r\n";
        var tokens = new CsvLexer(input);
        string[][] csvContent = CsvGrammar.CsvFile(tokens).Value;
            
Above, the array 'csvContent' will be equal to the following:
        
         new[]
         {
             new[] {"abc", "123", "DEF"},
             new[] {"ghi", "456", "JKL"}
         });