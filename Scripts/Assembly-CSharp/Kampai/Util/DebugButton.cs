using System.Collections.Generic;
using Kampai.Game;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class DebugButton : KampaiView
	{
		private bool visible;

		private GameObject instance;

		private bool captured;

		public Button button;

		public List<Image> images;

		private bool rightClickEnabled;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HUDChangedSiblingIndexSignal hudChangedSiblingIndexSignal { get; set; }

		[Inject]
		public DebugKeyHitSignal openSignal { get; set; }

		[Inject]
		public DebugPickService debugPickService { get; set; }

		[Inject]
		public DebugConsoleController controller { get; set; }

		[Inject]
		public DisplayDebugButtonSignal displayDebugButtonSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			hudChangedSiblingIndexSignal.AddListener(OnHudChangedIndex);
			openSignal.AddListener(ToggleOpen);
			controller.ToggleRightClickSignal.AddListener(OnToggleRightClick);
			displayDebugButtonSignal.AddListener(EnableButton);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			openSignal.RemoveListener(ToggleOpen);
			hudChangedSiblingIndexSignal.RemoveListener(OnHudChangedIndex);
			controller.ToggleRightClickSignal.RemoveListener(OnToggleRightClick);
			displayDebugButtonSignal.RemoveListener(EnableButton);
		}

		private void OnHudChangedIndex(int index)
		{
			base.transform.SetAsLastSibling();
		}

		public void OnClick(Button button)
		{
			ToggleOpen(DebugArgument.OPEN_CONSOLE);
		}

		private void EnableButton(bool enable)
		{
			button.enabled = enable;
			foreach (Image image in images)
			{
				image.enabled = enable;
			}
		}

		private void Update()
		{
			bool pressed = false;
			if (rightClickEnabled)
			{
				pressed = Input.GetTouch(0).phase == TouchPhase.Began;
			}
			debugPickService.OnGameInput(Input.mousePosition, 0, pressed);
		}

		private void OnToggleRightClick()
		{
			rightClickEnabled = !rightClickEnabled;
		}

		private void ToggleOpen(DebugArgument arg)
		{
			if (arg != 0)
			{
				return;
			}
			if (captured)
			{
				visible = instance.activeSelf;
			}
			if (!visible)
			{
				visible = true;
				if (!captured)
				{
					IGUICommand command = guiService.BuildCommand(GUIOperation.LoadStatic, "DebugConsole");
					instance = guiService.Execute(command);
					captured = true;
				}
				closeSignal.Dispatch(instance);
				instance.SetActive(visible);
			}
			else
			{
				visible = false;
				instance.SetActive(visible);
				closeSignal.Dispatch(null);
			}
		}
	}
}
