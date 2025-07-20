using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadInterfaceCommand : Command
	{
		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public AnimationToolkitModel Model { get; set; }

		[Inject]
		public Camera Camera { get; set; }

		[Inject]
		public Canvas Canvas { get; set; }

		[Inject]
		public LoadToggleGroupSignal LoadToggleGroupSignal { get; set; }

		[Inject]
		public LoadToggleSignal LoadToggleSignal { get; set; }

		[Inject]
		public InitToggleSignal InitToggleSignal { get; set; }

		public override void Execute()
		{
			Unload();
			if (Model.Mode == AnimationToolKitMode.Uninitialized)
			{
				LoadPrefab("MinionModeButton");
				LoadPrefab("VillainModeButton");
				LoadPrefab("CharacterModeButton");
			}
			else
			{
				LoadPrefab("ToggleInterfaceButton");
			}
			if (Model.Mode == AnimationToolKitMode.Building)
			{
				LoadBuildingToggles();
				LoadPrefab("ToggleMeshButton");
				LoadPrefab("AddMinionButton");
				LoadPrefab("RemoveMinionButton");
				LoadPrefab("LoopAnimationButton");
				LoadPrefab("GagAnimationButton");
				LoadPrefab("WaitAnimationButton");
			}
			else if (Model.Mode == AnimationToolKitMode.TikiBar)
			{
				LoadBuildingToggles();
				LoadPrefab("ToggleMeshButton");
				LoadPrefab("AddCharacterButton");
				LoadPrefab("RemoveCharacterButton");
				LoadPrefab("TikiBarCelebrateButton");
				LoadPrefab("TikiBarAttentionButton");
				LoadPrefab("TikiBarSpyGlassButton");
				LoadPrefab("TikiBarMixDrinkButton");
			}
			else if (Model.Mode == AnimationToolKitMode.Stage)
			{
				LoadBuildingToggles();
				LoadPrefab("ToggleMeshButton");
				LoadPrefab("AddCharacterButton");
				LoadPrefab("RemoveCharacterButton");
				LoadPrefab("StuartToggleIdleButton");
				LoadPrefab("StuartToggleAttentionButton");
				LoadPrefab("StuartPerformButton");
				LoadPrefab("StuartCelebrateButton");
			}
			else if (Model.Mode == AnimationToolKitMode.Minion)
			{
				Camera.transform.localPosition = new Vector3(10f, 10f, -10f);
				LoadPrefab("AddMinionButton");
				LoadPrefab("RemoveMinionButton");
				LoadPrefab("ScrollBoxPanel");
			}
			else if (Model.Mode == AnimationToolKitMode.Villain)
			{
				LoadToggleGroupSignal.Dispatch();
				int num = 0;
				IList<VillainDefinition> all = DefinitionService.GetAll<VillainDefinition>();
				foreach (VillainDefinition item in all)
				{
					LoadToggleSignal.Dispatch(num++ == 0, item.ID, item.LocalizedKey);
				}
				LoadPrefab("AddVillainButton");
				LoadPrefab("RemoveVillainButton");
				LoadPrefab("IntroAnimationButton");
				LoadPrefab("BoatAnimationButton");
				LoadPrefab("CabanaAnimationButton");
				LoadPrefab("FarewellAnimationButton");
			}
			else if (Model.Mode == AnimationToolKitMode.Character)
			{
				LoadToggleGroupSignal.Dispatch();
				int num2 = 0;
				IList<NamedCharacterDefinition> all2 = DefinitionService.GetAll<NamedCharacterDefinition>();
				foreach (NamedCharacterDefinition item2 in all2)
				{
					if (item2.Type != 0 && item2.Type != NamedCharacterType.TSM)
					{
						LoadToggleSignal.Dispatch(num2++ == 0, item2.ID, item2.LocalizedKey);
					}
				}
				LoadPrefab("AddCharacterButton");
				LoadPrefab("RemoveCharacterButton");
				LoadPrefab("ScrollBoxPanel");
			}
			else if (Model.Mode == AnimationToolKitMode.Debris)
			{
				LoadBuildingToggles();
				LoadPrefab("ToggleMeshButton");
				LoadPrefab("AddMinionButton");
				LoadPrefab("RemoveMinionButton");
			}
		}

		private void LoadPrefab(string resourcePath)
		{
			GameObject gameObject = Resources.Load<GameObject>(resourcePath);
			Vector3 position = gameObject.transform.position;
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.transform.parent = Canvas.transform;
			RectTransform rectTransform = gameObject2.transform as RectTransform;
			rectTransform.anchoredPosition = position;
		}

		private void Unload()
		{
			int num = 0;
			while (Canvas.transform.childCount > num)
			{
				GameObject gameObject = Canvas.transform.GetChild(num).gameObject;
				if (gameObject.GetComponent<ToggleGroup>() != null)
				{
					num++;
				}
				else
				{
					Object.DestroyImmediate(gameObject);
				}
			}
		}

		private void LoadBuildingToggles()
		{
			if (Object.FindObjectsOfType<ToggleGroup>().Length > 0)
			{
				return;
			}
			LoadToggleGroupSignal.Dispatch();
			int num = 0;
			ICollection<Building> instancesByType = PlayerService.GetInstancesByType<Building>();
			foreach (Building item in instancesByType)
			{
				LoadToggleSignal.Dispatch(num++ == 0, item.ID, item.Definition.LocalizedKey);
			}
		}
	}
}
