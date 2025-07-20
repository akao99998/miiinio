using System.Collections.Generic;

namespace Facebook.Unity
{
	internal class ShareResult : ResultBase, IResult, IShareResult
	{
		public string PostId { get; private set; }

		internal static string PostIDKey
		{
			get
			{
				return (!Constants.IsWeb) ? "id" : "post_id";
			}
		}

		internal ShareResult(ResultContainer resultContainer)
			: base(resultContainer)
		{
			if (ResultDictionary != null)
			{
				string value;
				if (ResultDictionary.TryGetValue<string>(PostIDKey, out value))
				{
					PostId = value;
				}
				else if (ResultDictionary.TryGetValue<string>("postId", out value))
				{
					PostId = value;
				}
			}
		}

		public override string ToString()
		{
			return Utilities.FormatToString(base.ToString(), GetType().Name, new Dictionary<string, string> { { "PostId", PostId } });
		}
	}
}
