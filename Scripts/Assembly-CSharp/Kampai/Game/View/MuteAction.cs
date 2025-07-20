using Kampai.Util;

namespace Kampai.Game.View
{
	public class MuteAction : KampaiAction
	{
		private ActionableObject obj;

		private bool muteStatus;

		public MuteAction(ActionableObject obj, bool muteStatus, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.muteStatus = muteStatus;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Execute()
		{
			AnimEventHandler[] componentsInChildren = obj.gameObject.GetComponentsInChildren<AnimEventHandler>();
			AnimEventHandler[] array = componentsInChildren;
			foreach (AnimEventHandler animEventHandler in array)
			{
				animEventHandler.mute = muteStatus;
			}
			base.Done = true;
		}
	}
}
