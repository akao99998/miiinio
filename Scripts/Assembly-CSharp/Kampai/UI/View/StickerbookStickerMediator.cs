using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class StickerbookStickerMediator : Mediator
	{
		[Inject]
		public StickerbookStickerView view { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(definitionService);
			view.buttonView.pointerDownSignal.AddListener(PointerDown);
			view.buttonView.pointerUpSignal.AddListener(PointerUp);
			pauseSignal.AddListener(OnPause);
		}

		public override void OnRemove()
		{
			view.buttonView.pointerDownSignal.RemoveListener(PointerDown);
			view.buttonView.pointerUpSignal.RemoveListener(PointerUp);
			pauseSignal.RemoveListener(OnPause);
		}

		private void PointerDown()
		{
			Vector3[] array = new Vector3[4];
			(view.transform as RectTransform).GetWorldCorners(array);
			Vector3 position = default(Vector3);
			Vector3[] array2 = array;
			foreach (Vector3 vector in array2)
			{
				position += vector;
			}
			position /= 4f;
			soundFXSignal.Dispatch("Play_menu_popUp_02");
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_StickerInfo");
			GUIArguments args = iGUICommand.Args;
			args.Add(view.stickerDefinition);
			args.Add(view.locked);
			args.Add(uiCamera.WorldToViewportPoint(position));
			guiService.Execute(iGUICommand);
		}

		private void PointerUp()
		{
			StartCoroutine(WaitASecond());
		}

		private IEnumerator WaitASecond()
		{
			yield return new WaitForEndOfFrame();
			hideItemPopupSignal.Dispatch();
		}

		private void OnPause()
		{
			hideItemPopupSignal.Dispatch();
		}
	}
}
