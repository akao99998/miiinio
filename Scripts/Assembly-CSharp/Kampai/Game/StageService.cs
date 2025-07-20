using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class StageService
	{
		private GameObject stageBackdrop;

		[Inject]
		public IPlayerService playerService { get; set; }

		public void ShowStageBackdrop()
		{
			if (stageBackdrop == null)
			{
				StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
				string backdropPrefabName = firstInstanceByDefinitionId.Definition.backdropPrefabName;
				GameObject original = KampaiResources.Load<GameObject>(backdropPrefabName);
				Vector3 position = new Vector3(firstInstanceByDefinitionId.Location.x, 0f, firstInstanceByDefinitionId.Location.y);
				stageBackdrop = Object.Instantiate(original);
				stageBackdrop.transform.position = position;
			}
			stageBackdrop.SetActive(true);
		}

		public void HideStageBackdrop()
		{
			if (stageBackdrop != null)
			{
				stageBackdrop.SetActive(false);
			}
		}
	}
}
