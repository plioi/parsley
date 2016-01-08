namespace Parsley
{
    public class Position : Value<Position>
    {
        public int Line { get; }
        public int Column { get; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        protected override object[] ImmutableFields()
        {
            return new object[] {Line, Column};
        }

        public override string  ToString()
        {
            return $"({Line}, {Column})";
        }
    }
}
