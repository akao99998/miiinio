namespace Kampai.UI.View
{
	public class MinionLevelTokenView : WorldToGlassView
	{
		public ButtonView HarvestButton;

		protected override string UIName
		{
			get
			{
				return "MinionLevelToken";
			}
		}

		internal void SetTokenCount(uint tokenCount)
		{
			base.gameObject.SetActive(tokenCount != 0);
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
		}
	}
}
