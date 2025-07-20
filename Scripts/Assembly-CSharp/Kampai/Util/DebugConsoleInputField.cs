using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class DebugConsoleInputField : InputField
	{
		public TouchScreenKeyboard Keyboard
		{
			get
			{
				return m_Keyboard;
			}
		}
	}
}
