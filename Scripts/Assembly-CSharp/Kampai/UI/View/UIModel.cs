using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.UI.View
{
	public class UIModel
	{
		[Flags]
		public enum UIStateFlags
		{
			StoreButtonHiddenFromQuest = 1
		}

		private struct UIStackElement
		{
			public int id;

			public Action callback;
		}

		private List<UIStackElement> objectStack = new List<UIStackElement>();

		public bool UIOpen
		{
			get
			{
				return objectStack.Count > 0;
			}
		}

		public bool AllowMultiTouch { get; set; }

		public bool CraftingUIOpen { get; set; }

		public bool DisableBack { get; set; }

		public bool LevelUpUIOpen { get; set; }

		public bool WelcomeBuddyOpen { get; set; }

		public bool StageUIOpen { get; set; }

		public bool LeisureMenuOpen { get; set; }

		public bool PopupAnimationIsPlaying { get; set; }

		public bool BuildingDragMode { get; set; }

		public bool GoToClicked { get; set; }

		public bool GoToInEffect { get; set; }

		public bool CaptainTeaserModalOpen { get; set; }

		public UIStateFlags UIState { get; set; }

		private void CheckAllowMultitouch()
		{
			Input.multiTouchEnabled = !UIOpen || AllowMultiTouch;
		}

		public void AddUI(int id, Action callback)
		{
			int num = objectStack.FindIndex((UIStackElement x) => x.id == id);
			if (num != -1)
			{
				objectStack.RemoveAt(num);
			}
			PopupAnimationIsPlaying = false;
			DisableBack = false;
			UIStackElement item = default(UIStackElement);
			item.id = id;
			item.callback = callback;
			objectStack.Insert(0, item);
			CheckAllowMultitouch();
		}

		public void RemoveUI(int id)
		{
			int num = objectStack.FindIndex((UIStackElement x) => x.id == id);
			if (num != -1)
			{
				objectStack.RemoveAt(num);
			}
			CheckAllowMultitouch();
		}

		public Action RemoveTopUI()
		{
			if (objectStack.Count > 0)
			{
				Action callback = objectStack[0].callback;
				objectStack.RemoveAt(0);
				CheckAllowMultitouch();
				return callback;
			}
			return null;
		}
	}
}
