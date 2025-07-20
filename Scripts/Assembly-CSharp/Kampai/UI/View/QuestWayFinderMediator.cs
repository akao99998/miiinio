namespace Kampai.UI.View
{
	public class QuestWayFinderMediator : AbstractQuestWayFinderMediator
	{
		[Inject]
		public QuestWayFinderView QuestWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return QuestWayFinderView;
			}
		}
	}
}
