namespace Kampai.Splash
{
	public interface ILoadInService
	{
		TipToShow GetNextTip();

		void SaveTipsForNextLaunch(int level);
	}
}
