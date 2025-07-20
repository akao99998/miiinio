using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ReInitializeGameCommand : Command
	{
		private static readonly IList<string> DO_NOT_DESTROY_GOS = new List<string> { "NimbleCallbackHelper", "UnityFacebookSDKPlugin", "PlayGames_QueueRunner" };

		[Inject]
		public string levelToLoad { get; set; }

		[Inject]
		public IAssetBundlesService assetBundlesService { get; set; }

		[Inject]
		public EnvironmentState state { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IAssetsPreloadService assetsPreloadService { get; set; }

		public override void Execute()
		{
			state.DisplayOn = false;
			state.EnvironmentBuilt = false;
			state.GridConstructed = false;
			Native.LogInfo(string.Format("ReInitializeGame, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			downloadService.Shutdown();
			KampaiView.ClearContextCache();
			assetsPreloadService.StopAssetsPreload();
			Object[] array = Object.FindObjectsOfType(typeof(GameObject));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = (GameObject)array[i];
				string name = gameObject.name;
				if (name == null || !DO_NOT_DESTROY_GOS.Contains(name))
				{
					Object.Destroy(gameObject);
				}
			}
			assetBundlesService.UnloadDLCBundles();
			assetBundlesService.UnloadSharedBundles();
			Go.killAllTweens();
			KampaiResources.ClearCache();
			LoadState.Set(LoadStateType.BOOTING);
			string sceneName = (string.IsNullOrEmpty(levelToLoad) ? "Initialize" : levelToLoad);
			SceneManager.LoadScene(sceneName);
		}
	}
}
