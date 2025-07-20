namespace Kampai.Game
{
	internal class PlayerSerializerV1 : DefaultPlayerSerializer
	{
		public override int Version
		{
			get
			{
				return 1;
			}
		}
	}
}
