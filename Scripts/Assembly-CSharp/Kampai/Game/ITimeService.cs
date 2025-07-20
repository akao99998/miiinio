using Kampai.Util;

namespace Kampai.Game
{
	public interface ITimeService
	{
		int CurrentTime();

		int Uptime();

		int AppTime();

		bool WithinRange(int a, int b);

		float RealtimeSinceStartup();

		bool WithinRange(IUTCRangeable rangeable, bool eternal = false);
	}
}
