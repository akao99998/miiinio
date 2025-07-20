using System.Collections;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class TownHallDialogView : KampaiView
	{
		public RectTransform CollectionHolder;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public UIRemovedSignal uiRemoveSignal { get; set; }

		public void AddMignetteScoreSummary(MignetteBuilding mignetteBuilding)
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "MignetteScoreSummary");
			GUIArguments args = iGUICommand.Args;
			args.Add(false);
			args.Add(mignetteBuilding.ID);
			args.Add(mignetteBuilding);
			GameObject gameObject = guiService.Execute(iGUICommand);
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.transform.SetParent(CollectionHolder, false);
			CollectionHolder.sizeDelta += new Vector2(component.GetComponent<LayoutElement>().minWidth + CollectionHolder.GetComponent<HorizontalLayoutGroup>().spacing, 0f);
			StartCoroutine(RemoveFromUIStack(gameObject));
		}

		private IEnumerator RemoveFromUIStack(GameObject scoreSummary)
		{
			yield return null;
			uiRemoveSignal.Dispatch(scoreSummary);
		}

		public void LeftAlignContent()
		{
			CollectionHolder.anchoredPosition = new Vector2(CollectionHolder.sizeDelta.x / 2f, 0f);
		}
	}
}
