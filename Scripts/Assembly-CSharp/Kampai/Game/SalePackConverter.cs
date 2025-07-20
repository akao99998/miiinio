using Kampai.Util;

namespace Kampai.Game
{
	public class SalePackConverter : FastJsonCreationConverter<SalePackDefinition>
	{
		public override SalePackDefinition Create()
		{
			return new SalePackDefinition();
		}
	}
}
