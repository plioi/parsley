namespace Parsley;

/// <summary>
/// Represents a void type, since `System.Void` is not valid everywhere a type name
/// is required, such as for return types or generic type arguments.
/// </summary>
public readonly struct Void
{
    static readonly Void _value;

    public static ref readonly Void Value => ref _value;
}
