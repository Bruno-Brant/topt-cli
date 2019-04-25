
using EasyTotp;
using System;

/**
* Clock input for the time-based OTPs (TOTP). The input is based on the current system time
* and is adjusted by a persistently stored correction value (offset in minutes).
*
* @author klyubin@google.com (Alex Klyubin)
*/
public class TotpClock
{

	private readonly NetworkTimeProvider mTimeProvider;

	//  private  SharedPreferences mPreferences;

	/**
     * Cached value of time correction (in minutes) or {@code null} if not cached. The value is cached
     * because it's read very frequently (once every 100ms) and is modified very infrequently.
     */
	private readonly int mTimeCorrectionSeconds;

	public TotpClock(NetworkTimeProvider timeProvider, int correctionSeconds)
	{
		mTimeProvider = timeProvider;
		mTimeCorrectionSeconds = correctionSeconds;
	}

	/**
     * Gets the number of milliseconds since epoch.
     */
	public long currentTimeMillis()
	{

		long timeMillis;

		//try
		//{
		//	timeMillis = mTimeProvider.getNetworkTime();
		//}
		//catch (Exception)
		//{
			timeMillis = DateTime.Now.currentTimeMillis();
		//}

		return timeMillis + mTimeCorrectionSeconds * 1000;
	}
}

public static class DateTimeExtensions
{
	private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	public static long currentTimeMillis(this DateTime d)
	{
		return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
	}
}
