namespace SquiggleCop.Tasks.Tests;

internal static class DateTimeExtensions
{
    public static bool IsCloseTo(this DateTime dateTime, DateTime anchor, TimeSpan precision)
    {
        return (dateTime + precision) >= anchor && (dateTime - precision) <= anchor;
    }
}
