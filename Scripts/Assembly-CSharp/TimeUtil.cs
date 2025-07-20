using System;

public static class TimeUtil
{
	private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static long CurrentTimeMillis()
	{
		return (long)(DateTime.UtcNow - EPOCH).TotalMilliseconds;
	}
}
