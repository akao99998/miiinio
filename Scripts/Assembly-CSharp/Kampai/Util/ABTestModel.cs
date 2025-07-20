using UnityEngine;

namespace Kampai.Util
{
	public class ABTestModel : MonoBehaviour
	{
		public static string configurationVariant = "anyVariant";

		public static string definitionURL;

		public static string definitionVariants;

		public static bool abtestEnabled;

		public static bool debugConsoleTest;

		public static void resetState()
		{
			definitionURL = null;
			abtestEnabled = false;
			configurationVariant = "anyVariant";
			debugConsoleTest = false;
		}
	}
}
