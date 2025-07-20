using System;
using Kampai.Util;

namespace Kampai.Game
{
	public class PartyDefinition
	{
		private Area normalized;

		public Area PartyArea { get; set; }

		public Location Center { get; set; }

		public float Radius { get; set; }

		public int Duration { get; set; }

		public float Percent { get; set; }

		public int StartAnimations { get; set; }

		public PartyDefinition()
		{
		}

		public PartyDefinition(int w, int h)
		{
			PartyArea = new Area(0, 0, w, h);
		}

		public PartyDefinition(int x1, int y1, int x2, int y2)
		{
			PartyArea = new Area(x1, y1, x2, y2);
		}

		private void AssertNormalized()
		{
			if (normalized == null)
			{
				Location a = PartyArea.a;
				Location b = PartyArea.b;
				int x = Math.Min(a.x, b.x);
				int y = Math.Min(a.y, b.y);
				int x2 = Math.Max(a.x, b.x);
				int y2 = Math.Max(a.y, b.y);
				normalized = new Area(x, y, x2, y2);
			}
		}

		public bool Contains(Point point)
		{
			AssertNormalized();
			Location a = normalized.a;
			Location b = normalized.b;
			return point.x >= a.x && point.y >= a.y && point.x <= b.x && point.y <= b.y;
		}
	}
}
