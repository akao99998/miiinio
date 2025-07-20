using Kampai.Game;

namespace Kampai.UI.View
{
	public class MasterPlanOnboardingView : PopupMenuView
	{
		public LocalizeView titleText;

		public ButtonView actionButtonView;

		public LocalizeView actionButtonText;

		internal MasterPlanOnboardDefinition definition;

		internal IDefinitionService definitionService;

		internal bool IsLast
		{
			get
			{
				return !definitionService.Has<MasterPlanOnboardDefinition>(definition.nextOnboardDefinitionId);
			}
		}

		internal void Initialize()
		{
			if (definition != null)
			{
				titleText.LocKey = definition.LocalizedKey;
				actionButtonText.LocKey = ((!IsLast) ? "Next" : "PlayerTrainingGotIt");
			}
		}
	}
}
