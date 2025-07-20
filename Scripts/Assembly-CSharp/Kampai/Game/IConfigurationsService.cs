using Ea.Sharkbite.HttpPlugin.Http.Api;

namespace Kampai.Game
{
	public interface IConfigurationsService
	{
		ConfigurationDefinition GetConfigurations();

		void GetConfigurationCallback(IResponse response);

		string GetConfigURL();

		void setInitonCallback(bool init);

		bool isKillSwitchOn(KillSwitch killswitchType);

		string GetConfigVariant();

		string GetDefinitionVariants();

		void OverrideKillswitch(KillSwitch killswitchType, bool killswitchValue);

		void ClearKillswitchOverride(KillSwitch killswitchType);

		void ClearAllKillswitchOverrides();
	}
}
