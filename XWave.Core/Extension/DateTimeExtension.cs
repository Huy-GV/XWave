namespace XWave.Core.Extension;

public static class DateTimeExtension
{
    public static bool IsBetween(this DateTime date, DateTime from, DateTime to)
    {
        if (from > to) throw new ArgumentException("The first date parameter must be before the second parameter");

        return date >= from && date <= to;
    }
}