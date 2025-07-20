using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;

namespace Kampai.Game.View
{
	public class EnvironmentalMignetteView : strange.extensions.mediation.impl.View
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("EnvironmentalMignetteView") as IKampaiLogger;

		public GameObject VfxPrefab;

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		private IPool<PoolableVFX> VfxPool { get; set; }

		protected override void Start()
		{
			base.Start();
			VfxPool = new Pool<PoolableVFX>();
			VfxPool.instanceProvider = new PoolableVfxProvider(VfxPrefab);
			VfxPool.size = 4;
			VfxPool.overflowBehavior = PoolOverflowBehavior.IGNORE;
		}

		public void AnimateEnvironmentalMignette(GameObject emoGO)
		{
			if (emoGO == null)
			{
				logger.Warning("Can't animate a null GO");
				return;
			}
			EnvironmentalMignetteObject component = emoGO.GetComponent<EnvironmentalMignetteObject>();
			if (component != null)
			{
				PlayEffect(component);
			}
			else
			{
				logger.Warning("GO " + emoGO.name + " doesn't have an EMO");
			}
		}

		private void PlayEffect(EnvironmentalMignetteObject emo)
		{
			emo.PlayEnvironmentalMignetteEffect(VfxPool);
			string audioStringConst = emo.GetAudioStringConst();
			if (!string.IsNullOrEmpty(audioStringConst))
			{
				globalAudioSignal.Dispatch(audioStringConst);
			}
		}
	}
}
