using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class PanInstructions
	{
		public int InstanceId { get; set; }

		public Instance Instance { get; set; }

		public Boxed<float> ZoomDistance { get; set; }

		public Boxed<Vector3> Offset { get; set; }

		public CameraMovementSettings CameraMovementSettings { get; set; }

		public PanInstructions(int instanceId)
		{
			InstanceId = instanceId;
		}

		public PanInstructions(Instance instance)
		{
			Instance = instance;
		}
	}
}
