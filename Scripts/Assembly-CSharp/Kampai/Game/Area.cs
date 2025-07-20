namespace Kampai.Game
{
	public class Area
	{
		public Location a { get; set; }

		public Location b { get; set; }

		public Area()
		{
		}

		public Area(int x1, int y1, int x2, int y2)
		{
			a = new Location(x1, y1);
			b = new Location(x2, y2);
		}
	}
}
