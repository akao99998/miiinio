using System.Collections.Generic;

namespace Kampai.UI.View
{
	public abstract class AbstractParentWayFinderView : AbstractWayFinderView, IParentWayFinderView, IWayFinderView, IWorldToGlassView
	{
		private Dictionary<int, IChildWayFinderView> _childrenWayFinders = new Dictionary<int, IChildWayFinderView>();

		public Dictionary<int, IChildWayFinderView> ChildrenWayFinders
		{
			get
			{
				return _childrenWayFinders;
			}
		}

		public override bool Snappable
		{
			set
			{
				base.Snappable = value;
				if (_childrenWayFinders == null)
				{
					return;
				}
				foreach (IChildWayFinderView value2 in _childrenWayFinders.Values)
				{
					value2.AvoidsHUD = AvoidsHUD;
				}
			}
		}

		private IChildWayFinderView GetChildWayFinder(int trackedId)
		{
			if (_childrenWayFinders != null && _childrenWayFinders.ContainsKey(trackedId))
			{
				return _childrenWayFinders[trackedId];
			}
			return null;
		}

		internal override void Clear()
		{
			logger.Info("Cleaning Parent Way Finder with tracked id: {0}", TrackedId);
			if (_childrenWayFinders != null)
			{
				_childrenWayFinders.Clear();
			}
		}

		public void AddChildWayFinder(IChildWayFinderView childWayFinderView)
		{
			int trackedId = childWayFinderView.TrackedId;
			if (GetChildWayFinder(trackedId) != null)
			{
				logger.Warning("Child way finder with id: {0} already exists as a child to parent way finder id: {1}", trackedId, TrackedId);
			}
			else
			{
				childWayFinderView.ParentWayFinderTrackedId = TrackedId;
				_childrenWayFinders.Add(trackedId, childWayFinderView);
			}
		}

		public void RemoveChildWayFinder(int childTrackedId)
		{
			IChildWayFinderView childWayFinder = GetChildWayFinder(childTrackedId);
			if (childWayFinder == null)
			{
				logger.Warning("Child way finder with id: {0} does not exist as a child to parent way finder id: {1}", childTrackedId, TrackedId);
			}
			else
			{
				childWayFinder.ParentWayFinderTrackedId = -1;
				_childrenWayFinders.Remove(childTrackedId);
			}
		}

		public void UpdateWayFinderIcon()
		{
			logger.Info("Updating parent way finder icon with id: {0}", TrackedId);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			foreach (IChildWayFinderView value in _childrenWayFinders.Values)
			{
				IQuestWayFinderView questWayFinderView = value as IQuestWayFinderView;
				if (questWayFinderView != null)
				{
					if (questWayFinderView.IsQuestComplete())
					{
						flag = true;
						break;
					}
					if (questWayFinderView.IsNewQuestAvailable())
					{
						flag2 = true;
					}
					else if (questWayFinderView.IsTaskReady())
					{
						flag4 = true;
					}
					else if (questWayFinderView.IsQuestAvailable())
					{
						flag3 = true;
					}
				}
			}
			if (flag)
			{
				UpdateIcon(wayFinderDefinition.QuestCompleteIcon);
			}
			else if (flag2)
			{
				UpdateIcon(wayFinderDefinition.NewQuestIcon);
			}
			else if (flag4)
			{
				UpdateIcon(wayFinderDefinition.TaskCompleteIcon);
			}
			else if (flag3)
			{
				UpdateIcon(WayFinderDefaultIcon);
			}
		}
	}
}
