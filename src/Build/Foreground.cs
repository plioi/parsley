namespace Build
{
    using System;

    public class Foreground : IDisposable
    {
        readonly ConsoleColor before;

        public Foreground(ConsoleColor color)
        {
            before = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public void Dispose()
        {
            Console.ForegroundColor = before;
        }

        public static Foreground Cyan => new Foreground(ConsoleColor.Cyan);
        public static Foreground Green => new Foreground(ConsoleColor.Green);
        public static Foreground DarkRed => new Foreground(ConsoleColor.DarkRed);
    }
}