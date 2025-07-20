using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class PreLoadPartyAssetsCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		public override void Execute()
		{
			IInjectionBinding binding = gameContext.injectionBinder.GetBinding<GameObject>(GameElement.PARTY_OBJECT);
			GameObject gameObject;
			if (binding == null)
			{
				gameObject = new GameObject(GameElement.PARTY_OBJECT.ToString());
				gameObject.transform.parent = contextView.transform;
				gameContext.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(GameElement.PARTY_OBJECT);
			}
			else
			{
				gameObject = binding.value as GameObject;
			}
			MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>();
			GameObject gameObject2 = CreateAssetGroup(gameObject, "StartPartyVFX");
			foreach (VFXAssetDefinition item in minionPartyDefinition.startPartyVFX)
			{
				GameObject original = KampaiResources.Load<GameObject>(item.Prefab);
				GameObject gameObject3 = Object.Instantiate(original);
				gameObject3.transform.parent = gameObject2.transform;
				gameObject3.transform.position = (Vector3)item.location;
			}
			gameObject2.SetActive(false);
			GameObject gameObject4 = CreateAssetGroup(gameObject, "BuildingVFX");
			foreach (VFXAssetDefinition partyVFXDefintion in minionPartyDefinition.partyVFXDefintions)
			{
				GameObject gameObject5 = LoadInstance(gameObject4, partyVFXDefintion.Prefab);
				gameObject5.transform.position = (Vector3)partyVFXDefintion.location;
			}
			gameObject4.SetActive(false);
		}

		private static GameObject CreateAssetGroup(GameObject parent, string assetGroup)
		{
			GameObject gameObject = parent.FindChild(assetGroup);
			if (gameObject == null)
			{
				gameObject = new GameObject(assetGroup);
				gameObject.transform.parent = parent.transform;
			}
			return gameObject;
		}

		private static GameObject LoadInstance(GameObject assetGroup, string prefabPath)
		{
			GameObject original = KampaiResources.Load<GameObject>(prefabPath);
			GameObject gameObject = Object.Instantiate(original);
			gameObject.transform.parent = assetGroup.transform;
			return gameObject;
		}
	}
}
