namespace Kampai.Util
{
	public static class Extensions
	{
		private static readonly FatalCode[] NETWORK = new FatalCode[17]
		{
			FatalCode.GS_ERROR_LOGIN,
			FatalCode.GS_ERROR_LOGIN_2,
			FatalCode.GS_ERROR_LOGIN_3,
			FatalCode.GS_ERROR_LOGIN_4,
			FatalCode.GS_ERROR_LOGIN_5,
			FatalCode.GS_ERROR_FETCHING_PLAYER_DATA,
			FatalCode.CMD_HTTP_CLIENT,
			FatalCode.CMD_SAVE_PLAYER,
			FatalCode.DS_UNABLE_TO_FETCH,
			FatalCode.CONFIG_NETWORK_FAIL,
			FatalCode.DLC_REQ_FAIL,
			FatalCode.GS_ERROR_REGISTER,
			FatalCode.GS_ERROR_DOWNLOAD_MANIFEST,
			FatalCode.GS_ERROR_FETCH_DEFINITIONS,
			FatalCode.GS_ERROR_LOAD_PLAYER,
			FatalCode.DS_UNABLE_TO_LOAD,
			FatalCode.CMD_LOAD_PLAYER
		};

		public static bool IsNetworkError(this FatalCode code)
		{
			FatalCode[] nETWORK = NETWORK;
			foreach (FatalCode fatalCode in nETWORK)
			{
				if (fatalCode == code)
				{
					return true;
				}
			}
			return false;
		}
	}
}
