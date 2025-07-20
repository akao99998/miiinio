using Kampai.Game;
using UnityEngine;

namespace Kampai.Main
{
	public class WhiteboxContext : MainContext
	{
		public WhiteboxContext()
		{
		}

		public WhiteboxContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override string PlayerDataSource()
		{
			return "whitebox_player";
		}

		protected override void BindPlayerCommand()
		{
			base.commandBinder.Bind<LoadPlayerSignal>().To<LoadWhiteboxPlayerCommand>();
			base.commandBinder.Bind<LoginUserSignal>().To<LoginWhiteboxUserCommand>();
		}
	}
}
