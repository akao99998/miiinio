using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class CraftingPopupMediator : AbstractGenericPopupMediator<CraftingPopupView>
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenuSignal { get; set; }

		public override void Register(ItemDefinition itemDef, Vector3 itemCenter)
		{
			base.view.Display(itemCenter, playerService, definitionService, base.localizationService, glassCanvas);
			base.view.SetName(base.localizationService.GetString(itemDef.LocalizedKey));
			IngredientsItemDefinition ingredientsItemDefinition = itemDef as IngredientsItemDefinition;
			if (ingredientsItemDefinition != null)
			{
				base.view.SetTime((int)ingredientsItemDefinition.TimeToHarvest);
				base.view.PopulateIngredients(ingredientsItemDefinition);
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			closeAllOtherMenuSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			closeAllOtherMenuSignal.RemoveListener(Close);
		}

		private void Close(GameObject exception)
		{
			OnMenuClose();
		}
	}
}
