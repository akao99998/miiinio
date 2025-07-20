using System.Collections.Generic;
using Kampai.Game.Mignette.View;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsCollectableViewObject : MonoBehaviour
	{
		public MignetteCollectableViewObject CollectableViewObject;

		private int pointValue;

		private float timeoutTimer;

		private EdwardMinionHandsMignetteManagerView parentView;

		private GoSpline spline;

		private bool isFlashing;

		private float flashTimer;

		private float travelPct;

		public void StartCollectable(Vector3 targetPos, int points, float time, EdwardMinionHandsMignetteManagerView parent)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(base.transform.position);
			Vector3 item = (base.transform.position + targetPos) / 2f;
			item.y += 2f;
			list.Add(item);
			list.Add(targetPos);
			spline = new GoSpline(list);
			spline.buildPath();
			travelPct = 0f;
			pointValue = points;
			timeoutTimer = time;
			parentView = parent;
			CollectableViewObject.GetComponent<Collider>().enabled = false;
			CollectableViewObject.UpdateMaterialForPointValue(points);
			CollectableViewObject.SetState(MignetteCollectableViewObject.CollectableStates.Spawn, parentView.mignetteCamera);
		}

		public void Update()
		{
			if (parentView == null || parentView.IsPaused)
			{
				return;
			}
			if (travelPct < 1f)
			{
				travelPct += Time.deltaTime * 1f;
				if (travelPct >= 1f)
				{
					travelPct = 1f;
					CollectableViewObject.GetComponent<Collider>().enabled = true;
					CollectableViewObject.SetState(MignetteCollectableViewObject.CollectableStates.Bounce, parentView.mignetteCamera);
				}
				Vector3 pointOnPath = spline.getPointOnPath(travelPct);
				base.transform.position = pointOnPath;
			}
			timeoutTimer -= Time.deltaTime;
			if (timeoutTimer <= 1f)
			{
				isFlashing = true;
			}
			if (timeoutTimer <= 0f)
			{
				parentView.CollectableHasTimedOut(this);
			}
			else if (isFlashing)
			{
				flashTimer -= Time.deltaTime;
				if (flashTimer <= 0f)
				{
					CollectableViewObject.ToggleModel();
					flashTimer = 0.1f;
				}
			}
		}

		public bool WasTapped()
		{
			if (CollectableViewObject.CollectableState == MignetteCollectableViewObject.CollectableStates.Collected)
			{
				return false;
			}
			CollectableViewObject.SetState(MignetteCollectableViewObject.CollectableStates.Collected, parentView.mignetteCamera);
			return true;
		}

		public int GetPointValue()
		{
			return pointValue;
		}
	}
}
