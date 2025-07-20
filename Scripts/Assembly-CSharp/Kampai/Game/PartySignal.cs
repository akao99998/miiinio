using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class PartySignal : RateLimitedSignal
	{
		public override float MinimumGap
		{
			get
			{
				return 1f;
			}
		}

		public override float CurrentTime
		{
			get
			{
				return Time.realtimeSinceStartup;
			}
		}
	}
}
