using UnityEngine;

namespace Kampai.Common
{
	public interface IPickService
	{
		void OnGameInput(Vector3 inputPosition, int input, bool pressed);

		void SetIgnoreInstanceInput(int instanceId, bool isIgnored);

		PickState GetPickState();
	}
}
