namespace Parsimonious
{
    public interface IReply<out T> : IReplyG
    {
        T Value { get; }
    }
}