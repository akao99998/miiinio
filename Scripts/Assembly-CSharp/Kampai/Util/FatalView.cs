using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class FatalView : MonoBehaviour
	{
		private static string code = "0000";

		private static string title = "FatalTitle";

		private static string message = "FatalMessage";

		private static string playerID = string.Empty;

		public Text ErrorCode;

		public Text ActionMessage;

		public Text TitleMessage;

		public Text PlayerID;

		private void OnEnable()
		{
			ErrorCode.text = code;
			ActionMessage.text = message;
			TitleMessage.text = title;
			PlayerID.text = playerID;
		}

		public static void SetFatalText(string code, string message, string title, string playerID)
		{
			FatalView.code = code;
			FatalView.message = message;
			FatalView.title = title;
			FatalView.playerID = playerID;
		}
	}
}
