using Kampai.Util;

namespace Kampai.Game
{
	public class PurchasedLandExpansionDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1059;
			}
		}

		public Instance Build()
		{
			return new PurchasedLandExpansion(this);
		}
	}
}
