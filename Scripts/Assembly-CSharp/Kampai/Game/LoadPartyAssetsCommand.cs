using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class LoadPartyAssetsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadPartyAssetsCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public SetMinionPartyBuildingStateSignal setMinionPartyBuildingStateSignal { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		public override void Execute()
		{
			if (guestService.PartyShouldProduceBuff())
			{
				MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>();
				IInjectionBinding binding = gameContext.injectionBinder.GetBinding<GameObject>(GameElement.PARTY_OBJECT);
				if (binding == null)
				{
					logger.Error("LoadPartyAssetsCommand: Cannot not find PARTY_OBJECT. Did you call PreloadPartyAssetsSignal first?");
					return;
				}
				GameObject gameObject = binding.value as GameObject;
				ActivateChild(gameObject, "StartPartyVFX");
				routineRunner.StartCoroutine(UnloadParticleSystem(gameObject, "StartPartyVFX"));
				routineRunner.StartCoroutine(StartBuildingPartyVFX(gameObject, minionPartyDefinition.PartyAssetDelay));
			}
		}

		private IEnumerator UnloadParticleSystem(GameObject parent, string assetGroup)
		{
			GameObject go = parent.FindChild(assetGroup);
			if (go == null)
			{
				yield break;
			}
			IList<ParticleSystem> particleSystems = go.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < particleSystems.Count; i++)
			{
				while (particleSystems[i] != null && particleSystems[i].IsAlive())
				{
					yield return null;
				}
			}
			Object.Destroy(go);
		}

		private void ActivateChild(GameObject parent, string childName)
		{
			GameObject gameObject = parent.FindChild(childName);
			if (gameObject == null)
			{
				logger.Error("LoadPartyAssetsCommand: can't find expected child: " + childName + " in parent: " + parent.name);
			}
			else
			{
				gameObject.SetActive(true);
			}
		}

		private IEnumerator StartBuildingPartyVFX(GameObject partyObject, float delay)
		{
			yield return new WaitForSeconds(delay);
			setMinionPartyBuildingStateSignal.Dispatch(true);
			ActivateChild(partyObject, "BuildingVFX");
		}
	}
}
