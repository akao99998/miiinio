namespace Kampai.UI.View
{
	public interface IGUICommand
	{
		GUIOperation operation { get; set; }

		GUIPriority priority { get; }

		string prefab { get; }

		bool WorldCanvas { get; set; }

		GUIArguments Args { get; set; }

		string GUILabel { get; set; }

		string skrimScreen { get; set; }

		bool darkSkrim { get; set; }

		float alphaAmt { get; set; }

		SkrimBehavior skrimBehavior { get; set; }

		bool disableSkrimButton { get; set; }

		bool singleSkrimClose { get; set; }

		bool genericPopupSkrim { get; set; }

		ShouldShowPredicateDelegate ShouldShowPredicate { get; set; }
	}
}
