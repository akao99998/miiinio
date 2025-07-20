using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class MessagePopupMediator : KampaiMediator
	{
		[Inject]
		public MessagePopupView view { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		[Inject]
		public MessageDialogClosed messageDialogClosed { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void OnRegister()
		{
			view.Init();
			closeAllDialogsSignal.AddListener(OnCloseDialogs);
			view.DialogClosedSignal.AddListener(OnDialogClosed);
		}

		public override void OnRemove()
		{
			closeAllDialogsSignal.RemoveListener(OnCloseDialogs);
			view.DialogClosedSignal.RemoveListener(OnDialogClosed);
		}

		public override void Initialize(GUIArguments args)
		{
			string text = args.Get<string>();
			bool flag = args.Get<bool>();
			MessagePopUpAnchor anchor = ((!args.Contains<MessagePopUpAnchor>()) ? MessagePopUpAnchor.TOP_RIGHT : args.Get<MessagePopUpAnchor>());
			Vector2 anchorPosition = ((!args.Contains<Vector2>()) ? Vector2.zero : args.Get<Vector2>());
			Tuple<float, float> tuple = args.Get<Tuple<float, float>>();
			if (tuple != null)
			{
				view.SetCustomTiming(tuple.Item1, tuple.Item2);
			}
			view.AutoClose = !flag;
			view.Display(text, anchor, anchorPosition);
		}

		private void OnCloseDialogs()
		{
			view.Show(false);
		}

		private void OnDialogClosed()
		{
			messageDialogClosed.Dispatch();
			guiService.Execute(GUIOperation.Unload, "popup_MessageBox");
		}
	}
}
