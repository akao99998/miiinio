using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class ToggleMediator : Mediator
	{
		private const float toggleHeight = 30f;

		private int toggleId;

		[Inject(AnimationToolKitElement.TOGGLE_GROUP)]
		public GameObject ToggleGroupGameObject { get; set; }

		[Inject]
		public ToggleView view { get; set; }

		[Inject]
		public AnimationToolKit AnimationToolKit { get; set; }

		public override void OnRegister()
		{
			base.transform.parent = ToggleGroupGameObject.transform;
			Toggle component = GetComponent<Toggle>();
			ToggleGroup toggleGroup = (component.group = ToggleGroupGameObject.GetComponent<ToggleGroup>());
			toggleGroup.RegisterToggle(component);
			Vector3 position = new Vector3(65f, 15f + (float)(ToggleGroupGameObject.transform.childCount - 1) * 30f);
			view.SetPosition(position);
			component.onValueChanged.AddListener(OnToggle);
		}

		public override void OnRemove()
		{
			Toggle component = GetComponent<Toggle>();
			component.onValueChanged.RemoveListener(OnToggle);
			ToggleGroup component2 = ToggleGroupGameObject.GetComponent<ToggleGroup>();
			component2.UnregisterToggle(component);
		}

		public void InitializeToggle(int instanceId, string labelText)
		{
			toggleId = instanceId;
			view.SetLabel(labelText);
			if (toggleId == 1000)
			{
				GetComponent<Toggle>().isOn = true;
			}
		}

		private void OnToggle(bool toggleOn)
		{
			if (toggleOn)
			{
				AnimationToolKit.ToggleOn(toggleId);
			}
		}
	}
}
