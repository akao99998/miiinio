using System;
using System.Text;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class SetAnimatorArgumentsAction : KampaiAction
	{
		protected ActionableObject obj;

		private object[] tuples;

		public SetAnimatorArgumentsAction(ActionableObject obj, IKampaiLogger logger, params object[] tuples)
			: base(logger)
		{
			this.obj = obj;
			if (tuples.Length % 2 != 0)
			{
				logger.Fatal(FatalCode.AN_ILLEGAL_ARGUMENT);
			}
			this.tuples = tuples;
		}

		public static void SetAnimationParameter(ActionableObject target, IKampaiLogger logger, string key, object value)
		{
			if (value is float || value is double)
			{
				target.SetAnimFloat(key, (float)value);
			}
			else if (value is int || value is uint || value is long)
			{
				target.SetAnimInteger(key, Convert.ToInt32(value));
			}
			else if (value is bool)
			{
				target.SetAnimBool(key, (bool)value);
			}
			else if (value == null)
			{
				target.SetAnimTrigger(key);
			}
			else if (value.ToString().Length == 0)
			{
				target.SetAnimBool(key, true);
			}
			else if (value is string)
			{
				bool result;
				if (bool.TryParse(value.ToString(), out result))
				{
					target.SetAnimBool(key, result);
					return;
				}
				logger.Error(string.Concat("Unknown animation argument: ", value, " (type=", value.GetType(), ")"));
			}
			else
			{
				logger.Error(string.Concat("Unknown animation argument: ", value, " (type=", value.GetType(), ")"));
			}
		}

		public override void Execute()
		{
			if (tuples != null)
			{
				for (int i = 0; i < tuples.Length; i += 2)
				{
					SetAnimationParameter(obj, logger, (string)tuples[i], tuples[i + 1]);
				}
			}
			base.Done = true;
		}

		public override string ToString()
		{
			string arg = "null";
			if (tuples != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < tuples.Length; i += 2)
				{
					string value = (string)tuples[i];
					object obj = tuples[i + 1];
					if (obj == null)
					{
						obj = "trigger";
					}
					stringBuilder.Append(value).Append("=").Append(obj.ToString())
						.Append(" ");
				}
				arg = stringBuilder.ToString();
			}
			return string.Format("{0} - {1}", GetType().Name, arg);
		}
	}
}
