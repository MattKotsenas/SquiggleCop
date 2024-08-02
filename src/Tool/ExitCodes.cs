namespace SquiggleCop.Tool;

internal static class ExitCodes
{
    public static int Success { get; } = 0;
    public static int UnknownError { get; } = -1;
    public static int BaselineMismatch { get; } = 1;
}
