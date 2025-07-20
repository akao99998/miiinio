using FMOD.Studio;
using UnityEngine;

namespace Kampai.Util
{
	public class Bootstrap : MonoBehaviour
	{
		private static bool HasBuggyBinarysShader()
		{
			string graphicsDeviceName = SystemInfo.graphicsDeviceName;
			return graphicsDeviceName.Contains("SGX") || graphicsDeviceName.Contains("225");
		}

		private void Awake()
		{
			if (HasBuggyBinarysShader())
			{
				Handheld.ClearShaderCache();
			}
			UnityUtil.ForceLoadLowLevelBinary();
			Screen.sleepTimeout = -2;
		}
	}
}
