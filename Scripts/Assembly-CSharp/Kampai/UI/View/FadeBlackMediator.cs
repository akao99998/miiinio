namespace Kampai.UI.View
{
	public class FadeBlackMediator : KampaiMediator
	{
		[Inject]
		public FadeBlackView view { get; set; }

		[Inject]
		public FadeBlackSignal fadeBlackSignal { get; set; }

		public override void OnRegister()
		{
			fadeBlackSignal.AddListener(view.Fade);
		}

		public override void OnRemove()
		{
			fadeBlackSignal.RemoveListener(view.Fade);
		}
	}
}
