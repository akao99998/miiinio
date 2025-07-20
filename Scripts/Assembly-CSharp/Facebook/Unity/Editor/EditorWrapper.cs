using Facebook.Unity.Editor.Dialogs;

namespace Facebook.Unity.Editor
{
	internal class EditorWrapper : IEditorWrapper
	{
		private IFacebookCallbackHandler callbackHandler;

		public EditorWrapper(IFacebookCallbackHandler callbackHandler)
		{
			this.callbackHandler = callbackHandler;
		}

		public void Init()
		{
			callbackHandler.OnInitComplete(string.Empty);
		}

		public void ShowLoginMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId, string permsisions)
		{
			MockLoginDialog component = ComponentFactory.GetComponent<MockLoginDialog>();
			component.Callback = callback;
			component.CallbackID = callbackId;
		}

		public void ShowAppRequestMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId)
		{
			ShowEmptyMockDialog(callback, callbackId, "Mock App Request");
		}

		public void ShowGameGroupCreateMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId)
		{
			ShowEmptyMockDialog(callback, callbackId, "Mock Game Group Create");
		}

		public void ShowGameGroupJoinMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId)
		{
			ShowEmptyMockDialog(callback, callbackId, "Mock Game Group Join");
		}

		public void ShowAppInviteMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId)
		{
			ShowEmptyMockDialog(callback, callbackId, "Mock App Invite");
		}

		public void ShowPayMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId)
		{
			ShowEmptyMockDialog(callback, callbackId, "Mock Pay");
		}

		public void ShowMockShareDialog(Utilities.Callback<ResultContainer> callback, string subTitle, string callbackId)
		{
			MockShareDialog component = ComponentFactory.GetComponent<MockShareDialog>();
			component.SubTitle = subTitle;
			component.Callback = callback;
			component.CallbackID = callbackId;
		}

		private void ShowEmptyMockDialog(Utilities.Callback<ResultContainer> callback, string callbackId, string title)
		{
			EmptyMockDialog component = ComponentFactory.GetComponent<EmptyMockDialog>();
			component.Callback = callback;
			component.CallbackID = callbackId;
			component.EmptyDialogTitle = title;
		}
	}
}
