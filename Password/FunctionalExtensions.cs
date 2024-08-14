namespace Password;

public static class FunctionalExtensions
{
    public static TResult Let<T, TResult>(this T value, Func<T, TResult> func) => func(value);
}