using System;

namespace Kampai.Util
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class RequiresJsonConverterAttribute : Attribute
	{
	}
}
