![Parsley](https://github.com/plioi/parsley/raw/master/parsley.png)
# Parsley

Parsley is a monadic parser combinator library inspired by Haskell's [Parsec](http://www.haskell.org/haskellwiki/Parsec) and F#'s [FParsec](http://www.quanttec.com/fparsec/).  It can parse context-sensitive, infinite look-ahead grammars but it performs best on predictive (LL[1]) grammars.

Unlike Parsec/FParsec, Parsley provides separate lexer/parser phases.  The lexer phase is usually performed with a prioritized list of regex patterns, and parser grammars are expressed in terms of the tokens produced by the lexer.

## Installation

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install Parsley from the package manager console:

    PM> Install-Package Parsley

## Lexer Phase (Tokenization)

Strings being parsed are represented with a `Text` instance, which tracks the original string as well as the current parsing position:

        var text = new Text("some input to parse");

The lexer phase is implemented by anything that produces an `IEnumerable<Token>`.  The default implementation, `Lexer`, builds the series of tokens when given a prioritized series of `TokenKind` token recognizers.  The most common `TokenKind` implementation is `Pattern`, which recognizes tokens via regex patterns.  `TokenKinds` can be skippable, if you want them to be recognized but discarded:

        var text = new Text("1 2 3 a b c");
        var lexer = new Lexer(new Patter("letter", @"[a-z]"),
                              new Pattern("number", @"[0-9]+"),
                              new Pattern("whitespace", @"\s+", skippable: true);

        Token[] tokens = lexer.ToArray();

Above, the array `tokens` will contain 6 `Token` objects. Each `Token` contains the literal ("1", "a", etc), the `TokenKind` that matched it, and the `Position` (line/column number) where the token was found.

The collection of `Token` produced by the lexer phase is wrapped in a `TokenStream`, which allows the rest of the system to traverse the collection of tokens in an immutable fashion.

## Parser Functions

A parser of thingies is a method that consumes a `TokenStream` and produces a parsed-thingy:

        public interface Parser<out T>
        {
            Reply<T> Parse(TokenStream tokens);
        }

A `Reply<T>` describes whether or not the parser succeeded, the parsed-thingy (on success), a possibly-empty error message list, and a reference to a `TokenStream` representing the remaining unparsed tokens.

## Grammars

Grammars should inherit from `Grammar` to take advantage of several `Parser` primitives.  Grammars should define each grammar rule in terms of these primitives, ultimately exposing the start rule as some `Parser<T>`.  Grammar rule bodies may consist of LINQ queries, which allow you to glue together other grammar rules in sequence:

See the integration tests for a [sample JSON grammar](https://github.com/plioi/parsley/tree/master/src/Parsley.Test/IntegrationTests/Json).

Finally, we can put all these pieces together to parse some text:

        const string input = "{\"zero\" : 0, \"one\" : 1, \"two\" : 2}";
        var tokens = new JsonLexer(input);
        var jsonDictionary = (Dictionary<string, object>) JsonGrammar.Json.Parse(tokens).Value;