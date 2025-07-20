using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class OpenRateAppPageCommand : Command
	{
		public override void Execute()
		{
			Application.OpenURL("market://details?id=com.ea.gp.minions");
		}
	}
}
