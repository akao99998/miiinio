using System;

namespace Kampai.Util
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface)]
	public sealed class DeserializerAttribute : Attribute
	{
		public string Signature { get; private set; }

		public DeserializerAttribute(string signature)
		{
			Signature = signature;
		}
	}
}
