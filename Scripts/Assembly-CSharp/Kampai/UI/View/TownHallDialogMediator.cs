using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class TownHallDialogMediator : UIStackMediator<TownHallDialogView>
	{
		private bool clickedOnSkrim;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public OnClickSkrimSignal onClickSkrimSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			onClickSkrimSignal.AddListener(OnClickSkrim);
			List<int> list = new List<int>();
			IList<MignetteBuilding> instancesByType = playerService.GetInstancesByType<MignetteBuilding>();
			foreach (MignetteBuilding item in instancesByType)
			{
				int iD = item.Definition.ID;
				if (!list.Contains(iD))
				{
					list.Add(iD);
					base.view.AddMignetteScoreSummary(item);
				}
			}
			List<MignetteBuilding> list2 = new List<MignetteBuilding>();
			IList<Building> allAspirationalBuildings = landExpansionService.GetAllAspirationalBuildings();
			foreach (Building item2 in allAspirationalBuildings)
			{
				MignetteBuilding mignetteBuilding = item2 as MignetteBuilding;
				int iD = mignetteBuilding.Definition.ID;
				if (mignetteBuilding != null && !list.Contains(iD))
				{
					list.Add(iD);
					list2.Add(mignetteBuilding);
					base.view.AddMignetteScoreSummary(mignetteBuilding);
				}
			}
			base.view.LeftAlignContent();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			onClickSkrimSignal.RemoveListener(OnClickSkrim);
		}

		private void OnClickSkrim()
		{
			clickedOnSkrim = true;
		}

		protected override void Close()
		{
			hideSkrim.Dispatch("TownHallSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_TownHall");
			if (clickedOnSkrim)
			{
				displayPlayerTrainingSignal.Dispatch(19000008, false, new Signal<bool>());
			}
			soundFXSignal.Dispatch("Play_menu_disappear_01");
		}
	}
}
