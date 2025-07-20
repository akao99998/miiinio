using UnityEngine;

namespace Kampai.Game
{
	public class SocialUser
	{
		public string name { get; set; }

		public string email { get; set; }

		protected Texture image { get; set; }

		protected Vector2 uvOffset { get; set; }

		public void SetTexture(Texture image, Vector2 uvOffset)
		{
			this.image = image;
			this.uvOffset = uvOffset;
		}

		public Texture GetTexture()
		{
			return image;
		}
	}
}
