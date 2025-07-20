using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class StorageBuildingItemInfoMediator : KampaiMediator
	{
		[Inject]
		public StorageBuildingItemInfoView view { get; set; }

		[Inject]
		public ILocalizationService LocalizationService { get; set; }

		[Inject]
		public RemoveStorageBuildingItemDescriptionSignal removeItemDescriptionSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			removeItemDescriptionSignal.AddListener(Close);
			view.OnMenuClose.AddListener(OnMenuClose);
			view.gotoButton.ClickedSignal.AddListener(GotoPressed);
			view.Init(LocalizationService, uiCamera);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.gotoButton.ClickedSignal.RemoveListener(GotoPressed);
			removeItemDescriptionSignal.RemoveListener(Close);
			view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			ItemDefinition itemDefinition = args.Get<ItemDefinition>();
			RectTransform itemCenter = args.Get<RectTransform>();
			Vector3 center = args.Get<Vector3>();
			view.SetItem(itemDefinition, itemCenter, center, definitionService);
		}

		private void Close()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			view.Close();
		}

		private void OnMenuClose()
		{
			Object.Destroy(base.gameObject);
		}

		private void GotoPressed()
		{
			Close();
			gotoSignal.Dispatch(view.ItemDefinition.ID);
		}
	}
}
