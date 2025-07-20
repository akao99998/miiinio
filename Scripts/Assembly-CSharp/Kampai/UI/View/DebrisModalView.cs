using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class DebrisModalView : PopupMenuView
	{
		public Text TitleText;

		public Text MinionsCountText;

		public Text AvailableText;

		public GameObject DebrisModalItemPrefab;

		public Transform DebrisModalItemContainer;

		private DebrisModalItemView modalItemView;

		private string itemImage;

		private string itemMask;

		private int itemsRequired;

		public int MinionsAvailable { get; private set; }

		internal void Init(int minionsAvailable, int itemsAvailable, int itemsRequired, string image, string mask, ILocalizationService localService, DebrisBuildingDefinition definition)
		{
			base.Init();
			this.itemsRequired = itemsRequired;
			UpdateAvailableMinions(minionsAvailable);
			string @string = localService.GetString(definition.LocalizedKey);
			string string2 = localService.GetString("ClearX?", @string);
			TitleText.text = string2;
			AvailableText.text = localService.GetString("ResourceAvailable");
			itemImage = image;
			itemMask = mask;
			CreateItem(itemsAvailable);
			base.Open();
		}

		internal void UpdateAvailableMinions(int minionsAvailable)
		{
			MinionsAvailable = minionsAvailable;
			MinionsCountText.text = string.Format("{0}", minionsAvailable);
		}

		internal void OnDragItemOverDropArea(DragDropItemView dragDropItemView, bool success)
		{
			DebrisModalItemView component = dragDropItemView.GetComponent<DebrisModalItemView>();
			component.Highlight(success);
		}

		internal void OnDropItemOverDropArea(DragDropItemView dragDropItemView, bool success)
		{
			DebrisModalItemView component = dragDropItemView.GetComponent<DebrisModalItemView>();
			component.Highlight(false);
			component.ResetPosition(!success);
		}

		private void CreateItem(int itemsAvailable)
		{
			GameObject gameObject = Object.Instantiate(DebrisModalItemPrefab);
			gameObject.transform.SetParent(DebrisModalItemContainer, false);
			modalItemView = gameObject.GetComponent<DebrisModalItemView>();
			modalItemView.Init(itemImage, itemMask, itemsAvailable, itemsRequired);
		}
	}
}
