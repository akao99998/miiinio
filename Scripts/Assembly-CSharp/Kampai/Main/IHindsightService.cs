namespace Kampai.Main
{
	public interface IHindsightService
	{
		void Initialize();

		void UpdateCache();

		HindsightCampaign GetCachedContent(HindsightCampaign.Scope scope);
	}
}
