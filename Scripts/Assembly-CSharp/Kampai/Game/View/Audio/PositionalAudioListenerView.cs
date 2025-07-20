using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View.Audio
{
	public class PositionalAudioListenerView : KampaiView
	{
		public void UpdatePosition(Vector3 newPosition)
		{
			base.transform.position = newPosition;
		}
	}
}
