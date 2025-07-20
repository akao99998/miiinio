namespace Kampai.Main
{
	public class LocalString : ILocalString
	{
		private string value;

		public LocalString(string value)
		{
			this.value = value;
		}

		public string GetStringFormat(params object[] args)
		{
			if (args != null && args.Length > 0)
			{
				return string.Format(value, args);
			}
			return value;
		}

		public string GetString()
		{
			return value;
		}

		public void SetKeyValue(string tag, string tagValue)
		{
			value = value.Replace(tag, tagValue);
		}
	}
}
