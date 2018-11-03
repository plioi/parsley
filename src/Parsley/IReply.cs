namespace Parsley
{
    public interface IReply<out T> : IReplyG
    {
        T Value { get; }
    }
}