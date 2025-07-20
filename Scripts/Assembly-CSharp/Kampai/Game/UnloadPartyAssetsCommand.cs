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
	public class UnloadPartyAssetsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UnloadPartyAssetsCommand") as IKampaiLogger;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public SetMinionPartyBuildingStateSignal setMinionPartyBuildingStateSignal { get; set; }

		public override void Execute()
		{
			IInjectionBinding binding = gameContext.injectionBinder.GetBinding<GameObject>(GameElement.PARTY_OBJECT);
			if (binding == null)
			{
				logger.Warning("Trying to unload party assets when they haven't been loaded");
				return;
			}
			GameObject partyObject = binding.value as GameObject;
			MinionPartyDefinition definition = definitionService.Get<MinionPartyDefinition>();
			LoadPartyEndVFX(partyObject, definition);
			routineRunner.StartCoroutine(UnloadPartyObjects(partyObject, definition));
		}

		private void LoadPartyEndVFX(GameObject partyObject, MinionPartyDefinition definition)
		{
			GameObject gameObject = new GameObject("EndPartyVFX");
			gameObject.transform.parent = partyObject.transform;
			foreach (VFXAssetDefinition item in definition.endPartyVFX)
			{
				GameObject original = KampaiResources.Load<GameObject>(item.Prefab);
				GameObject gameObject2 = Object.Instantiate(original);
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.position = (Vector3)item.location;
			}
			routineRunner.StartCoroutine(UnloadParticleSystem(partyObject, "EndPartyVFX"));
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
				while (particleSystems[i].IsAlive())
				{
					yield return null;
				}
			}
			Object.Destroy(go);
		}

		private IEnumerator UnloadPartyObjects(GameObject partyObject, MinionPartyDefinition definition)
		{
			yield return new WaitForSeconds(definition.PartyAssetDelay);
			DestroyAssetGroup(partyObject, "BuildingVFX");
			setMinionPartyBuildingStateSignal.Dispatch(false);
		}

		private static void DestroyAssetGroup(GameObject parent, string assetGroup)
		{
			GameObject gameObject = parent.FindChild(assetGroup);
			if (!(gameObject == null))
			{
				Object.Destroy(gameObject);
			}
		}
	}
}
