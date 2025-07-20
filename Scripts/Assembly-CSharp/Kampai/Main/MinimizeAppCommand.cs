using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class MinimizeAppCommand : Command
	{
		public override void Execute()
		{
			Native.Exit();
		}
	}
}
