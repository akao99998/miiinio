using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Mignette.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

public class AutoSavePlayerStateCommand : Command
{
	public int autoSaveInterval = 60;

	[Inject]
	public SavePlayerSignal savePlayerSignal { get; set; }

	[Inject(SocialServices.FACEBOOK)]
	public ISocialService facebookService { get; set; }

	[Inject(SocialServices.GOOGLEPLAY)]
	public ISocialService googlePlayService { get; set; }

	[Inject]
	public ICoppaService coppaService { get; set; }

	[Inject]
	public IConfigurationsService configService { get; set; }

	[Inject]
	public LogClientMetricsSignal clientMetricsSignal { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	public override void Execute()
	{
		routineRunner.StartCoroutine(PeriodicSavePlayer());
	}

	private IEnumerator PeriodicSavePlayer()
	{
		while (true)
		{
			yield return new WaitForSeconds(autoSaveInterval);
			while (MignetteManagerView.GetIsPlaying())
			{
				yield return new WaitForSeconds(1f);
			}
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, false));
			clientMetricsSignal.Dispatch(false);
			SetAutoSaveInterval();
		}
	}

	public void SetAutoSaveInterval()
	{
		ConfigurationDefinition configurations = configService.GetConfigurations();
		bool isLoggedIn = facebookService.isLoggedIn;
		isLoggedIn |= !coppaService.Restricted() && googlePlayService.isLoggedIn;
		autoSaveInterval = ((!isLoggedIn) ? configurations.autoSaveIntervalUnlinkedAccount : configurations.autoSaveIntervalLinkedAccount);
		if (autoSaveInterval == 0)
		{
			autoSaveInterval = 60;
		}
	}
}
