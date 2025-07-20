using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSelectorListItemView : KampaiView
	{
		public Text Title;

		public Image Background;

		private BuildingDefinition buildingDefiniition;

		public Signal<BuildingDefinition> ClickedSignal = new Signal<BuildingDefinition>();

		private bool modified;

		[Inject]
		public BuildingModifiedSignal buildingModifiedSignal { get; set; }

		[Inject]
		public BuildingsStateSavedSignal buildingsStateSavedSignal { get; set; }

		internal void Setup(BuildingDefinition def)
		{
			buildingDefiniition = def;
			Title.text = string.Format("{0}: {1}", def.ID, def.LocalizedKey);
			updateColor();
		}

		protected override void Start()
		{
			base.Start();
			buildingModifiedSignal.AddListener(delegate(BuildingDefinition d)
			{
				if (d == buildingDefiniition)
				{
					modified = true;
					updateColor();
				}
			});
			buildingsStateSavedSignal.AddListener(delegate
			{
				modified = false;
				updateColor();
			});
		}

		public void OnClick()
		{
			ClickedSignal.Dispatch(buildingDefiniition);
		}

		private void updateColor()
		{
			if (buildingDefiniition.UiScale < Mathf.Epsilon || buildingDefiniition.UiPosition == Vector3.zero)
			{
				Background.color = Color.red;
			}
			else if (modified)
			{
				Background.color = Color.cyan;
			}
			else
			{
				Background.color = Color.white;
			}
		}
	}
}
