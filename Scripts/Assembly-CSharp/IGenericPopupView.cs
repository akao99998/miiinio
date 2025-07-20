using Kampai.Main;
using UnityEngine;

public interface IGenericPopupView
{
	void Init(ILocalizationService localizationService);

	void Display(Vector3 itemCenter);

	void Close(bool instant);
}
