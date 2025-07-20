using Kampai.Util;
using UnityEngine;

namespace Kampai.Fatal
{
	public class FatalContext : BaseContext
	{
		public FatalContext()
		{
		}

		public FatalContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void MapBindings()
		{
		}
	}
}
