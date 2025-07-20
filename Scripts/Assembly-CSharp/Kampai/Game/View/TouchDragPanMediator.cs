using UnityEngine;

namespace Kampai.Game.View
{
	public class TouchDragPanMediator : PanMediator
	{
		private int curFingerID = -1;

		[Inject]
		public TouchDragPanView view { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		public override void OnGameInput(Vector3 position, int input)
		{
			if (blocked)
			{
				return;
			}
			if (input == 1)
			{
				if (curFingerID == -1)
				{
					CacheFingerID();
				}
				if (curFingerID != GetFingerID())
				{
					CacheFingerID();
					view.ResetBehaviour();
				}
			}
			if (input == 1)
			{
				view.CalculateBehaviour(position);
			}
			else
			{
				view.ResetBehaviour();
			}
			view.PerformBehaviour(position);
		}

		public override void OnDisableBehaviour(int behaviour)
		{
			int num = 8;
			if ((behaviour & num) == num)
			{
				if (!blocked)
				{
					blocked = true;
				}
				if ((model.CurrentBehaviours & num) == num)
				{
					model.CurrentBehaviours ^= num;
				}
			}
		}

		public override void OnEnableBehaviour(int behaviour)
		{
			int num = 8;
			if ((behaviour & num) == num)
			{
				if (blocked)
				{
					blocked = false;
				}
				if ((model.CurrentBehaviours & num) != num)
				{
					model.CurrentBehaviours ^= num;
				}
			}
		}

		private void CacheFingerID()
		{
			curFingerID = GetFingerID();
		}
	}
}
