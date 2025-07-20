using System.Collections.Generic;
using Facebook.MiniJSON;

namespace Facebook.Unity
{
	internal class ResultContainer
	{
		private const string CanvasResponseKey = "response";

		public string RawResult { get; private set; }

		public IDictionary<string, object> ResultDictionary { get; set; }

		public ResultContainer(IDictionary<string, object> dictionary)
		{
			RawResult = dictionary.ToJson();
			ResultDictionary = dictionary;
			if (Constants.IsWeb)
			{
				ResultDictionary = GetWebFormattedResponseDictionary(ResultDictionary);
			}
		}

		public ResultContainer(string result)
		{
			RawResult = result;
			if (string.IsNullOrEmpty(result))
			{
				ResultDictionary = new Dictionary<string, object>();
				return;
			}
			ResultDictionary = Json.Deserialize(result) as Dictionary<string, object>;
			if (Constants.IsWeb && ResultDictionary != null)
			{
				ResultDictionary = GetWebFormattedResponseDictionary(ResultDictionary);
			}
		}

		private IDictionary<string, object> GetWebFormattedResponseDictionary(IDictionary<string, object> resultDictionary)
		{
			IDictionary<string, object> value;
			if (resultDictionary.TryGetValue<IDictionary<string, object>>("response", out value))
			{
				object value2;
				if (resultDictionary.TryGetValue("callback_id", out value2))
				{
					value["callback_id"] = value2;
				}
				return value;
			}
			return resultDictionary;
		}
	}
}
