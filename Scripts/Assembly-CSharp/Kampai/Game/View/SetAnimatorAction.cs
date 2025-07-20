using System.Collections.Generic;
using System.Text;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class SetAnimatorAction : KampaiAction
	{
		protected ActionableObject obj;

		private RuntimeAnimatorController controller;

		private Dictionary<string, object> animationParams;

		public SetAnimatorAction(ActionableObject obj, RuntimeAnimatorController controller, IKampaiLogger logger, Dictionary<string, object> animationParams = null)
			: base(logger)
		{
			this.obj = obj;
			this.controller = controller;
			this.animationParams = animationParams;
		}

		public SetAnimatorAction(ActionableObject obj, RuntimeAnimatorController controller, string paramName, IKampaiLogger logger, object paramValue = null)
			: base(logger)
		{
			this.obj = obj;
			this.controller = controller;
			animationParams = new Dictionary<string, object>();
			animationParams.Add(paramName, paramValue);
		}

		public override void Execute()
		{
			if (controller != null)
			{
				obj.SetAnimController(controller);
			}
			if (animationParams != null)
			{
				foreach (KeyValuePair<string, object> animationParam in animationParams)
				{
					SetAnimatorArgumentsAction.SetAnimationParameter(obj, logger, animationParam.Key, animationParam.Value);
				}
			}
			base.Done = true;
		}

		public override string ToString()
		{
			string arg = "null";
			if (controller != null)
			{
				arg = controller.name;
			}
			string arg2 = "null";
			if (animationParams != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string key in animationParams.Keys)
				{
					object obj = animationParams[key];
					if (obj == null)
					{
						obj = "trigger";
					}
					stringBuilder.Append(key).Append("=").Append(obj.ToString())
						.Append(" ");
				}
				arg2 = stringBuilder.ToString();
			}
			return string.Format("{0} - {1} params: {2}", GetType().Name, arg, arg2);
		}
	}
}
