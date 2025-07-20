using System;

namespace Kampai.Util
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class DebugCommandAttribute : Attribute
	{
		public string Name { get; set; }

		public string[] Args { get; set; }

		public bool RequiresAllArgs { get; set; }

		public DebugCommandAttribute()
		{
			Name = null;
		}
	}
}
