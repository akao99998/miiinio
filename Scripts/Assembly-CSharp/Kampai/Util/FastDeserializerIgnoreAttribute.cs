using System;

namespace Kampai.Util
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class FastDeserializerIgnoreAttribute : Attribute
	{
	}
}
