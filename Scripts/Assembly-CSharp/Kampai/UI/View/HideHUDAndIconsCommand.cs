using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class HideHUDAndIconsCommand : Command
	{
		[Inject]
		public bool isVisible { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public SetHUDButtonsVisibleSignal setHUDButtonsVisibleSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ShowAllResourceIconsSignal showAllResourceIconsSignal { get; set; }

		[Inject]
		public HideAllResourceIconsSignal hideAllResourceIconsSignal { get; set; }

		[Inject]
		public ToggleAllFloatingTextSignal toggleAllFloatingTextSignal { get; set; }

		public override void Execute()
		{
			setBuildMenuEnabledSignal.Dispatch(isVisible);
			setHUDButtonsVisibleSignal.Dispatch(isVisible);
			toggleAllFloatingTextSignal.Dispatch(isVisible);
			if (isVisible)
			{
				showAllWayFindersSignal.Dispatch();
				showAllResourceIconsSignal.Dispatch();
			}
			else
			{
				hideAllWayFindersSignal.Dispatch();
				hideAllResourceIconsSignal.Dispatch();
			}
		}
	}
}
