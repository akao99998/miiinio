using System;

namespace Kampai.Util
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface)]
	public sealed class SerializerAttribute : Attribute
	{
		public string Signature { get; private set; }

		public SerializerAttribute(string signature)
		{
			Signature = signature;
		}
	}
}
