namespace Parsley;

public delegate Reply<T> Parser<out T>(ref Text input);
