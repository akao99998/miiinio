using System.Collections.Generic;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Game.Mignette.View
{
	public class MignetteBuildingViewObject : MonoBehaviour
	{
		public bool UsesTimerHUD = true;

		public bool UsesProgressHUD;

		public bool UsesCounterHUD;

		public float PreCountdownDelay;

		public bool UseCountDownTimer = true;

		protected ICollection<GameObject> DynamicCoolDownObjects = new LinkedList<GameObject>();

		public virtual void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
		{
		}

		public virtual void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
		{
		}

		public void AddDynamicCoolDownObject(GameObject go)
		{
			DynamicCoolDownObjects.Add(go);
		}

		public void DestroyDynamicCoolDownObjects()
		{
			foreach (GameObject dynamicCoolDownObject in DynamicCoolDownObjects)
			{
				Object.Destroy(dynamicCoolDownObject);
			}
			if (DynamicCoolDownObjects.Count > 0)
			{
				DynamicCoolDownObjects = new LinkedList<GameObject>();
			}
		}

		public bool IsDynamicCooldownObjectsLoaded()
		{
			return DynamicCoolDownObjects.Count > 0;
		}
	}
}
