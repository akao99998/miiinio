using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSelectorView : KampaiView
	{
		private static readonly HashSet<BuildingType.BuildingTypeIdentifier> allowedBuildingTypes = new HashSet<BuildingType.BuildingTypeIdentifier>
		{
			BuildingType.BuildingTypeIdentifier.CRAFTING,
			BuildingType.BuildingTypeIdentifier.DECORATION,
			BuildingType.BuildingTypeIdentifier.LEISURE,
			BuildingType.BuildingTypeIdentifier.RESOURCE,
			BuildingType.BuildingTypeIdentifier.SPECIAL,
			BuildingType.BuildingTypeIdentifier.MASTER_COMPONENT,
			BuildingType.BuildingTypeIdentifier.MASTER_LEFTOVER
		};

		public GameObject ScrollContent;

		public BuildingsSelectorListItemView ListItemViewBase;

		public Text Title;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public BuildingSelectedSignal buildingSelectedSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			List<BuildingDefinition> all = definitionService.GetAll<BuildingDefinition>();
			foreach (BuildingDefinition item in all)
			{
				if (allowedBuildingTypes.Contains(item.Type))
				{
					BuildingsSelectorListItemView buildingsSelectorListItemView = Object.Instantiate(ListItemViewBase);
					buildingsSelectorListItemView.Setup(item);
					buildingsSelectorListItemView.gameObject.SetActive(true);
					buildingsSelectorListItemView.transform.SetParent(ScrollContent.transform, false);
					buildingsSelectorListItemView.ClickedSignal.AddListener(buildingSelected);
				}
			}
			StartCoroutine(loadFirstBuilding(all[0]));
		}

		private IEnumerator loadFirstBuilding(BuildingDefinition def)
		{
			yield return null;
			buildingSelected(def);
		}

		private void buildingSelected(BuildingDefinition def)
		{
			Title.text = string.Format("{0}: {1}", def.ID, def.LocalizedKey);
			buildingSelectedSignal.Dispatch(def);
		}
	}
}
