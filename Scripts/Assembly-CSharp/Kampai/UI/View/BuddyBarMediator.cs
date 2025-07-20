using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class BuddyBarMediator : UIStackMediator<BuddyBarView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("BuddyBarMediator") as IKampaiLogger;

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public LoadBuddyBarSignal loadSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.SkrimButtonView.ClickedSignal.AddListener(Close);
			loadSignal.AddListener(Load);
			base.gameObject.SetActive(false);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.SkrimButtonView.ClickedSignal.RemoveListener(Close);
			loadSignal.RemoveListener(Load);
		}

		private void Load()
		{
			if (base.view.IsOpen())
			{
				return;
			}
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			IList<Prestige> buddyPrestiges = prestigeService.GetBuddyPrestiges();
			if (buddyPrestiges != null)
			{
				int count = buddyPrestiges.Count;
				base.view.SetupRowCount(count);
				for (int i = 0; i < count; i++)
				{
					BuddyAvatarView buddyAvatarView = BuddyAvatarBuilder.Build(buddyPrestiges[i], prestigeService, logger);
					base.view.AddItem(buddyAvatarView, i);
				}
				base.view.InitScrollView(count);
			}
		}

		protected override void Close()
		{
			playSFXSignal.Dispatch("Play_button_click_01");
			base.view.Close();
		}
	}
}
