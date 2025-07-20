using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View
{
	public class GenericPopupMediator : AbstractGenericPopupMediator<GenericPopupView>
	{
		private ItemDefinition itemDefinition;

		private bool showGoto;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			base.soundFXSignal.Dispatch("Play_menu_popUp_02");
			itemDefinition = args.Get<ItemDefinition>();
			Vector3 itemCenter = args.Get<Vector3>();
			showGoto = args.Get<bool>();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			Register(itemDefinition, itemCenter);
		}

		public override void Register(ItemDefinition itemDef, Vector3 itemCenter)
		{
			base.view.Display(itemCenter);
			string itemOrigin = string.Empty;
			if (itemDef != null)
			{
				base.view.SetName(base.localizationService.GetString(itemDef.LocalizedKey));
				IngredientsItemDefinition ingredientsItemDefinition = itemDef as IngredientsItemDefinition;
				if (ingredientsItemDefinition != null)
				{
					base.view.SetTime(IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(ingredientsItemDefinition, definitionService));
					int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(itemDef.ID);
					itemOrigin = base.localizationService.GetString(definitionService.Get<BuildingDefinition>(buildingDefintionIDFromItemDefintionID).LocalizedKey);
				}
				else
				{
					DropItemDefinition dropItemDefinition = itemDef as DropItemDefinition;
					if (dropItemDefinition != null)
					{
						itemOrigin = base.localizationService.GetString("StorageBuildingTooltipRandomDrop");
						base.view.DisableDurationInfo();
					}
				}
			}
			base.view.SetItemOrigin(itemOrigin);
			if (showGoto)
			{
				base.view.ShowGotoButton();
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.gotoButton.ClickedSignal.AddListener(gotoClicked);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.gotoButton.ClickedSignal.RemoveListener(gotoClicked);
		}

		private void gotoClicked()
		{
			base.closeSignal.Dispatch();
			gotoSignal.Dispatch(itemDefinition.ID);
		}
	}
}
