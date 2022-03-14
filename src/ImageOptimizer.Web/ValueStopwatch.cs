using System.Diagnostics;

namespace ImageOptimizer.Web;

public struct ValueStopwatch
{
    private readonly long _startTimestamp;
    private static readonly double _timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    private ValueStopwatch(long startTimestamp)
        => _startTimestamp = startTimestamp;

    public static ValueStopwatch StartNew()
        => new(Stopwatch.GetTimestamp());

    public TimeSpan GetElapsedTime()
    {
        // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
        // So it being 0 is a clear indication of default(ValueStopwatch)
        if (_startTimestamp == 0)
        {
            throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
        }

        var end = Stopwatch.GetTimestamp();
        var timestampDelta = end - _startTimestamp;
        var ticks = (long)(_timestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }
}
