using System.Collections.Generic;

namespace Facebook.Unity
{
	internal class GroupCreateResult : ResultBase, IGroupCreateResult, IResult
	{
		public const string IDKey = "id";

		public string GroupId { get; private set; }

		public GroupCreateResult(ResultContainer resultContainer)
			: base(resultContainer)
		{
			string value;
			if (ResultDictionary != null && ResultDictionary.TryGetValue<string>("id", out value))
			{
				GroupId = value;
			}
		}

		public override string ToString()
		{
			return Utilities.FormatToString(base.ToString(), GetType().Name, new Dictionary<string, string> { { "GroupId", GroupId } });
		}
	}
}
