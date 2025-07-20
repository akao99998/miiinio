using UnityEngine;

namespace Kampai.Game.View
{
	public class KeyboardDragPanMediator : PanMediator
	{
		[Inject]
		public KeyboardDragPanView view { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		public override void OnGameInput(Vector3 position, int input)
		{
			if (!blocked)
			{
				if (((uint)input & (true ? 1u : 0u)) != 0)
				{
					view.CalculateBehaviour(position);
				}
				else
				{
					view.ResetBehaviour();
				}
				view.PerformBehaviour(position);
			}
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
	}
}
