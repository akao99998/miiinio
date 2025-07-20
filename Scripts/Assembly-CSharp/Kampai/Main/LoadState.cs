namespace Kampai.Main
{
	public static class LoadState
	{
		private static LoadStateType type;

		public static LoadStateType Get()
		{
			return type;
		}

		public static void Set(LoadStateType newState)
		{
			type = newState;
		}
	}
}
