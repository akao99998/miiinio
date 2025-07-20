using UnityEngine;
using UnityEngine.UI;

public class QualityCycler : MonoBehaviour
{
	public Text ButtonText;

	private int qualityLevel;

	private void Start()
	{
		qualityLevel = QualitySettings.GetQualityLevel();
		ButtonText.text = QualitySettings.names[qualityLevel];
	}

	public void Cycle()
	{
		qualityLevel++;
		if (qualityLevel >= QualitySettings.names.Length)
		{
			qualityLevel = 0;
		}
		QualitySettings.SetQualityLevel(qualityLevel, true);
		ButtonText.text = QualitySettings.names[qualityLevel];
	}
}
