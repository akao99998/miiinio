using System;

namespace Kampai.Game
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class QuestScriptAPIAttribute : Attribute
	{
		public string Name { get; private set; }

		public QuestScriptAPIAttribute(string name)
		{
			Name = name;
		}
	}
}
