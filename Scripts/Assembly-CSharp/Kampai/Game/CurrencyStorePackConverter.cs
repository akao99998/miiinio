using Kampai.Util;

namespace Kampai.Game
{
	public class CurrencyStorePackConverter : FastJsonCreationConverter<CurrencyStorePackDefinition>
	{
		public override CurrencyStorePackDefinition Create()
		{
			return new CurrencyStorePackDefinition();
		}
	}
}
