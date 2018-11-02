namespace Parsley
{
    public interface IReply<out T> : IGeneralReply
    {
        T Value { get; }
    }
}