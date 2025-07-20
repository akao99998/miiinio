using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSizeToolkitValueEditView : MonoBehaviour
	{
		public InputField TextField;

		public float IncrementStep = 0.1f;

		private float currentValue;

		public BuildingsSizeToolkitValueUpdateButton[] ValueUpdateButtons;

		public Signal<float> ValueChangedSignal = new Signal<float>();

		public float CurrentValue
		{
			get
			{
				return currentValue;
			}
			set
			{
				currentValue = value;
				TextField.text = value.ToString();
			}
		}

		public void Start()
		{
			BuildingsSizeToolkitValueUpdateButton[] valueUpdateButtons = ValueUpdateButtons;
			foreach (BuildingsSizeToolkitValueUpdateButton buildingsSizeToolkitValueUpdateButton in valueUpdateButtons)
			{
				buildingsSizeToolkitValueUpdateButton.UpdateValueSignal.AddListener(UpdateValue);
			}
		}

		private void UpdateValue(float sign)
		{
			CurrentValue += sign * IncrementStep;
			ValueChangedSignal.Dispatch(currentValue);
		}

		public void OnValueChanged(string value)
		{
			float result = 0f;
			float.TryParse(value, out result);
			if (Mathf.Abs(result - currentValue) > Mathf.Epsilon)
			{
				CurrentValue = result;
				ValueChangedSignal.Dispatch(result);
			}
		}
	}
}
