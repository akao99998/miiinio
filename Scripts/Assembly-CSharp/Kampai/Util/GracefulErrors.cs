using System.Collections.Generic;

namespace Kampai.Util
{
	public static class GracefulErrors
	{
		private static readonly Dictionary<FatalCode, GracefulMessage> graceful = new Dictionary<FatalCode, GracefulMessage>
		{
			{
				FatalCode.SESSION_INVALID,
				new GracefulMessage("DuplicateLogin", "DuplicateLoginDetail")
			},
			{
				FatalCode.EX_INSUFFICIENT_STORAGE,
				new GracefulMessage("InsufficientStorage", "InsufficientStorageMessage")
			},
			{
				FatalCode.GS_ERROR_CORRUPT_SAVE_DETECTED,
				new GracefulMessage("CorruptionTitle", "CorruptionMessage")
			}
		};

		public static bool IsGracefulError(FatalCode code)
		{
			return graceful.ContainsKey(code);
		}

		public static GracefulMessage GetGracefulError(FatalCode code)
		{
			return (!graceful.ContainsKey(code)) ? null : graceful[code];
		}
	}
}
