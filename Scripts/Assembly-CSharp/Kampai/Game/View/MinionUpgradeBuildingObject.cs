using System.Collections;
using Kampai.Main;
using Kampai.Util.Audio;
using UnityEngine;

namespace Kampai.Game.View
{
	public class MinionUpgradeBuildingObject : BuildingObject, IStartAudio
	{
		private PlayLocalAudioSignal playLocalAudioSignal;

		private bool playing;

		public void InitAudio(BuildingState creationState, PlayLocalAudioSignal playLocalAudioSignal)
		{
			this.playLocalAudioSignal = playLocalAudioSignal;
			StartCoroutine(WaitForGameToStart(creationState));
		}

		public void NotifyBuildingState(BuildingState newState)
		{
			if (newState != BuildingState.Broken && newState != BuildingState.Inaccessible)
			{
				StartLoop();
			}
		}

		private void StartLoop()
		{
			if (playLocalAudioSignal != null && !playing)
			{
				playLocalAudioSignal.Dispatch(Kampai.Util.Audio.GetAudioEmitter.Get(base.gameObject, "MinionUpgradeBuildingBuzz"), "Play_minionUpgradeBuilding_neon_01", null);
				playing = true;
			}
		}

		private IEnumerator WaitForGameToStart(BuildingState creationState)
		{
			yield return new WaitForEndOfFrame();
			NotifyBuildingState(creationState);
		}

		protected override Vector3 GetIndicatorPosition(bool centerY)
		{
			return new Vector3(minColliderY.bounds.center.x, minColliderY.bounds.max.y, minColliderY.bounds.center.z);
		}
	}
}
