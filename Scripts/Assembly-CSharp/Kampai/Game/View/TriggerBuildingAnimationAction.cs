using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class TriggerBuildingAnimationAction : KampaiAction
	{
		private AnimatingBuildingObject obj;

		private Dictionary<string, object> animationParams;

		private string mecanimState;

		public TriggerBuildingAnimationAction(AnimatingBuildingObject obj, Dictionary<string, object> animationParams, IKampaiLogger logger, string mecanimState = null)
			: base(logger)
		{
			this.obj = obj;
			this.animationParams = animationParams;
			this.mecanimState = mecanimState;
		}

		public TriggerBuildingAnimationAction(AnimatingBuildingObject obj, string triggerName, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			animationParams = new Dictionary<string, object>();
			animationParams.Add(triggerName, string.Empty);
		}

		public override void Execute()
		{
			if (animationParams != null)
			{
				foreach (KeyValuePair<string, object> animationParam in animationParams)
				{
					if (animationParam.Value is bool)
					{
						obj.SetAnimBool(animationParam.Key, (bool)animationParam.Value);
						if ((bool)animationParam.Value)
						{
							obj.SetVFXState(animationParam.Key, mecanimState);
						}
					}
					else if (animationParam.Value.ToString().Length == 0)
					{
						obj.SetAnimBool(animationParam.Key, true);
						obj.SetVFXState(animationParam.Key, mecanimState);
					}
					else if (animationParam.Value is string)
					{
						bool result;
						if (bool.TryParse(animationParam.Value.ToString(), out result))
						{
							obj.SetAnimBool(animationParam.Key, result);
							if (result)
							{
								obj.SetVFXState(animationParam.Key, mecanimState);
							}
						}
						else
						{
							logger.Error(string.Concat("Unknown animation argument: ", animationParam.Value, " (type=", animationParam.Value.GetType(), ")"));
						}
					}
					else
					{
						logger.Error(string.Concat("Unknown animation argument: ", animationParam.Value, " (type=", animationParam.Value.GetType(), ")"));
					}
				}
			}
			base.Done = true;
		}
	}
}
