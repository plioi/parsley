namespace Parsley
{
    public class Position : Value<Position>
    {
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        protected override object[] ImmutableFields()
        {
            return new object[] {Line, Column};
        }
    }
}
