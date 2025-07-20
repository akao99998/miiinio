namespace Kampai.Common
{
	public struct TelemetryParameter
	{
		public string keyType;

		public object value;

		public ParameterName name;

		public TelemetryParameter(string keyType, object value, ParameterName name)
		{
			this.keyType = keyType;
			this.value = value;
			this.name = name;
		}
	}
}
