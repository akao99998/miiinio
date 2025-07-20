using Elevation.Logging;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class TikiBarViewPickController : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TikiBarViewPickController") as IKampaiLogger;

		[Inject]
		public GameObject EndHitObject { get; set; }

		[Inject]
		public ITikiBarService TikiBarService { get; set; }

		[Inject]
		public DisplayStickerbookSignal DisplayStickerbookSignal { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public GetWayFinderSignal GetWayFinderSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal ShowQuestPanelSignal { get; set; }

		[Inject]
		public IQuestService QuestService { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManagerGO { get; set; }

		public override void Execute()
		{
			if (EndHitObject.name == "StampAlbum")
			{
				logger.Debug("StampAlbum was clicked in Tiki bar view!");
				DisplayStickerbookSignal.Dispatch();
				return;
			}
			if (EndHitObject.name == "Shelve" || EndHitObject.name == "building_313")
			{
				logger.Debug("Shelve was clicked in Tiki bar view, or Tikibar was clicked while Zoomed Out!");
				HandleClick(NamedCharacterManagerGO.GetComponent<NamedCharacterManagerView>().Get(78));
				return;
			}
			CharacterObject componentInParent = EndHitObject.GetComponentInParent<CharacterObject>();
			if (componentInParent != null)
			{
				logger.Debug("{0} was clicked in Tiki bar view!", componentInParent.name);
				HandleClick(componentInParent);
			}
			else
			{
				logger.Error("{0} clicked event was ignored!", EndHitObject.name);
			}
		}

		private void HandleClick(CharacterObject characterObject)
		{
			GetWayFinderSignal.Dispatch(78, delegate(int trackedId, IWayFinderView wayFinderView)
			{
				if (wayFinderView != null)
				{
					ClickedOnCharacter(characterObject, wayFinderView);
				}
				else
				{
					ShowQuestBook();
				}
			});
		}

		private void ShowQuestBook()
		{
			foreach (IQuestController value in QuestService.GetQuestMap().Values)
			{
				QuestDefinition definition = value.Definition;
				if (definition.SurfaceType != QuestSurfaceType.Automatic && definition.SurfaceType != QuestSurfaceType.ProcedurallyGenerated && value.State == QuestState.RunningTasks)
				{
					ShowQuestPanelSignal.Dispatch(value.ID);
					break;
				}
			}
		}

		private void ClickedOnCharacter(CharacterObject characterObject, IWayFinderView wayFinder)
		{
			int iD = characterObject.ID;
			Character byInstanceId = PlayerService.GetByInstanceId<Character>(iD);
			if (byInstanceId == null)
			{
				logger.Warning("Could not find named character for instance id:{0} ", iD);
			}
			else if (TikiBarService.IsCharacterSitting(byInstanceId))
			{
				wayFinder.SimulateClick();
			}
			else
			{
				logger.Warning("Ignoring clicks to {0} since they are not sitting", characterObject.name);
			}
		}
	}
}
