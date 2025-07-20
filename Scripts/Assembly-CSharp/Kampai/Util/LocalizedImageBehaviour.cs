using System.Collections.Generic;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class LocalizedImageBehaviour : MonoBehaviour
	{
		public List<Texture> Alternates;

		private void Start()
		{
			RawImage component = GetComponent<RawImage>();
			string text = component.texture.name;
			Texture texture = null;
			if (Alternates != null)
			{
				string resourcePath = HALService.GetResourcePath(Native.GetDeviceLanguage());
				foreach (Texture alternate in Alternates)
				{
					if (alternate != null && alternate.name != null && alternate.name.Equals(text + "_" + resourcePath))
					{
						texture = alternate;
						break;
					}
				}
			}
			if (texture != null)
			{
				component.texture = texture;
			}
		}
	}
}
