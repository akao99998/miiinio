using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PropObject : Object
	{
		public GameObject gameObject { get; set; }

		public List<Transform> transforms { get; set; }

		public PropObject()
		{
			gameObject = null;
			transforms = new List<Transform>();
		}
	}
}
