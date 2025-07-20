using Kampai.Game.View;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class FloatingTextView : WorldToGlassView
	{
		internal Signal OnRemoveSignal = new Signal();

		private Text descriptionText;

		protected override string UIName
		{
			get
			{
				return "FloatingText";
			}
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
			FloatingTextModal floatingTextModal = modal as FloatingTextModal;
			if (floatingTextModal == null)
			{
				logger.Error("Text WayFinder modal doesn't exist!");
				return;
			}
			descriptionText = floatingTextModal.DescriptionText;
			FloatingTextSettings floatingTextSettings = floatingTextModal.Settings as FloatingTextSettings;
			descriptionText.text = floatingTextSettings.descriptionText;
		}

		public override Vector3 GetIndicatorPosition()
		{
			BuildingObject buildingObject = targetObject as BuildingObject;
			if (buildingObject != null)
			{
				return buildingObject.IndicatorPosition;
			}
			return Vector3.zero;
		}

		public void SetHeight(float height)
		{
			RectTransform component = GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(component.sizeDelta.x, height);
		}

		protected override void OnHide()
		{
			base.OnHide();
			HideText();
		}

		protected override void OnShow()
		{
			base.OnShow();
			ShowText();
		}

		private void ShowText()
		{
			descriptionText.enabled = true;
		}

		private void HideText()
		{
			descriptionText.enabled = false;
		}

		internal override void TargetObjectNullResponse()
		{
			logger.Warning("Removing FloatingText with id: {0} since the target object does not exist", m_trackedId);
			OnRemoveSignal.Dispatch();
		}
	}
}
