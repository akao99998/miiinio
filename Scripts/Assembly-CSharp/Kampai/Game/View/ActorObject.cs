using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;

namespace Kampai.Game.View
{
	public class ActorObject<T> : ActionableObject where T : Instance
	{
		protected Agent agent;

		protected float defaultMaxSpeed;

		public GameObject BlobShadow { get; set; }

		public virtual void Init(T instance, IKampaiLogger logger)
		{
			ID = instance.ID;
			base.logger = logger;
			agent = base.gameObject.GetComponent<Agent>();
			defaultMaxSpeed = agent.MaxSpeed;
		}

		public Agent GetAgent()
		{
			return agent;
		}

		public void EnableBlobShadow(bool enabled)
		{
			if (BlobShadow != null)
			{
				BlobShadow.SetActive(enabled);
			}
		}

		public override void ExecuteAction(KampaiAction action)
		{
			agent.MaxSpeed = 0f;
			base.ExecuteAction(action);
		}

		public void setLocation(Vector3 position)
		{
			base.transform.position = position;
		}

		public void setRotation(Vector3 rotation)
		{
			base.transform.localEulerAngles = rotation;
		}
	}
}
