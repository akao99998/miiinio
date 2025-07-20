using System.Collections.Generic;

namespace Facebook.Unity
{
	internal class AccessTokenRefreshResult : ResultBase, IAccessTokenRefreshResult, IResult
	{
		public AccessToken AccessToken { get; private set; }

		public AccessTokenRefreshResult(ResultContainer resultContainer)
			: base(resultContainer)
		{
			if (ResultDictionary != null && ResultDictionary.ContainsKey(LoginResult.AccessTokenKey))
			{
				AccessToken = Utilities.ParseAccessTokenFromResult(ResultDictionary);
			}
		}

		public override string ToString()
		{
			return Utilities.FormatToString(base.ToString(), GetType().Name, new Dictionary<string, string> { 
			{
				"AccessToken",
				AccessToken.ToStringNullOk()
			} });
		}
	}
}
