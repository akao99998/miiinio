using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class RandomDropView : KampaiView
	{
		private const float heightOffset = 2f;

		private const float endOffset = 4f;

		public KampaiImage image;

		public ButtonView button;

		private Vector3 localScale;

		private float timeTillTween = 1.5f;

		private float pulseValue;

		private Vector3 scaleDelta;

		public Signal<Vector3, int> timeToTweenSignal = new Signal<Vector3, int>();

		public float InitialPulseValue { get; set; }

		public int ItemDefinitionId { get; set; }

		internal void Init()
		{
			localScale = base.gameObject.transform.localScale;
			scaleDelta = default(Vector3);
			GameObject gameObject = base.gameObject;
			List<Vector3> nodes = CreatePath();
			GoSpline path = new GoSpline(nodes);
			Go.to(gameObject.transform, 0.5f, new GoTweenConfig().setEaseType(GoEaseType.Linear).positionPath(path).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
			}));
			Go.to(this, 0.2f, new GoTweenConfig().floatProp("InitialPulseValue", 0.2f).setIterations(-1, GoLoopType.PingPong).setUpdateType(GoUpdateType.LateUpdate)
				.onBegin(delegate
				{
					pulseValue = InitialPulseValue;
				})
				.onUpdate(delegate
				{
					float f = InitialPulseValue - pulseValue;
					UpdatePulse(f);
					pulseValue = InitialPulseValue;
				}));
		}

		public void Update()
		{
			timeTillTween -= Time.deltaTime;
			if (timeTillTween <= 0f)
			{
				timeToTweenSignal.Dispatch(base.gameObject.transform.position, ItemDefinitionId);
			}
		}

		private void UpdatePulse(float f)
		{
			scaleDelta.x = f;
			scaleDelta.y = f;
			scaleDelta.z = f;
			base.gameObject.transform.localScale += scaleDelta;
		}

		internal void UpdateScale(float percentage)
		{
			base.gameObject.transform.localScale = localScale * (1f - percentage);
		}

		private List<Vector3> CreatePath()
		{
			List<Vector3> list = new List<Vector3>();
			Vector3 position = base.gameObject.transform.position;
			Vector3 item = new Vector3(position.x + 2f, position.y + 2f, position.z);
			Vector3 item2 = new Vector3(position.x + 4f, position.y - 2f, position.z);
			list.Add(position);
			list.Add(item);
			list.Add(item2);
			return list;
		}

		internal void KillTweens()
		{
			Go.killAllTweensWithTarget(base.gameObject.transform);
		}
	}
}
