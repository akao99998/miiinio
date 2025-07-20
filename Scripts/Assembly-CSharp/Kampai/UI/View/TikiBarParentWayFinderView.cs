namespace Kampai.UI.View
{
	public class TikiBarParentWayFinderView : AbstractParentWayFinderView
	{
		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.TikibarDefaultIcon;
			}
		}

		protected override string UIName
		{
			get
			{
				return "TikiBarParentWayFinder";
			}
		}

		protected override bool OnCanUpdate()
		{
			if (ChildrenWayFinders.Values.Count < 1)
			{
				return true;
			}
			if (isForceHideEnabled)
			{
				foreach (IChildWayFinderView value in ChildrenWayFinders.Values)
				{
					value.SetForceHide(true);
				}
				return false;
			}
			foreach (IChildWayFinderView value2 in ChildrenWayFinders.Values)
			{
				value2.SetForceHide(false);
			}
			return false;
		}
	}
}
