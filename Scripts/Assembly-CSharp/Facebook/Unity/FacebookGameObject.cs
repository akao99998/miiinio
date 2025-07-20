using UnityEngine;

namespace Facebook.Unity
{
	internal abstract class FacebookGameObject : MonoBehaviour, IFacebookCallbackHandler
	{
		public IFacebookImplementation Facebook { get; set; }

		public void Awake()
		{
			Object.DontDestroyOnLoad(this);
			AccessToken.CurrentAccessToken = null;
			OnAwake();
		}

		public void OnInitComplete(string message)
		{
			Facebook.OnInitComplete(new ResultContainer(message));
		}

		public void OnLoginComplete(string message)
		{
			Facebook.OnLoginComplete(new ResultContainer(message));
		}

		public void OnLogoutComplete(string message)
		{
			Facebook.OnLogoutComplete(new ResultContainer(message));
		}

		public void OnGetAppLinkComplete(string message)
		{
			Facebook.OnGetAppLinkComplete(new ResultContainer(message));
		}

		public void OnGroupCreateComplete(string message)
		{
			Facebook.OnGroupCreateComplete(new ResultContainer(message));
		}

		public void OnGroupJoinComplete(string message)
		{
			Facebook.OnGroupJoinComplete(new ResultContainer(message));
		}

		public void OnAppRequestsComplete(string message)
		{
			Facebook.OnAppRequestsComplete(new ResultContainer(message));
		}

		public void OnShareLinkComplete(string message)
		{
			Facebook.OnShareLinkComplete(new ResultContainer(message));
		}

		protected virtual void OnAwake()
		{
		}
	}
}
