using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PanView : KampaiView, CameraView
	{
		protected Vector3 currentPosition;

		protected Vector3 previousPosition;

		protected Vector3 velocity;

		protected float decayAmount;

		protected Camera mainCamera;

		protected Ray mouseRay;

		protected Plane groundPlane;

		protected float hitDistance;

		protected Vector3 hitPosition;

		protected bool initialized;

		protected Vector3 xVector = new Vector3(1f, 0f, 0f);

		protected Vector3 zVector = new Vector3(0f, 0f, 1f);

		private Vector3 multiplier;

		public Vector3 Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
			}
		}

		public float DecayAmount
		{
			get
			{
				return decayAmount;
			}
			set
			{
				decayAmount = value;
			}
		}

		protected override void Start()
		{
			mainCamera = base.gameObject.GetComponent<Camera>();
			decayAmount = 0.925f;
			Vector3 inNormal = new Vector3(0f, 1f, 0f);
			Vector3 inPoint = new Vector3(0f, 0f, 0f);
			groundPlane = new Plane(inNormal, inPoint);
			Quaternion quaternion = Quaternion.Euler(0f, base.transform.eulerAngles.y, 0f);
			xVector = quaternion * xVector;
			zVector = quaternion * zVector;
			base.Start();
		}

		public virtual void PerformBehaviour(CameraUtils cameraUtils)
		{
			base.transform.position = new Vector3(Mathf.Clamp(base.transform.position.x + velocity.x, 25f, 233f), base.transform.position.y + velocity.y, Mathf.Clamp(base.transform.position.z + velocity.z, -9f, 205f));
		}

		public virtual void CalculateBehaviour(Vector3 position)
		{
		}

		public virtual void ResetBehaviour()
		{
			initialized = false;
		}

		public virtual void Decay()
		{
			velocity *= decayAmount;
		}

		public virtual void SetupAutoPan(Vector3 panTo)
		{
			multiplier = panTo - base.transform.position;
		}

		public virtual void PerformAutoPan(float delta)
		{
			Vector3 vector = delta * multiplier;
			base.transform.position += new Vector3(vector.x, 0f, vector.z);
			velocity = Vector3.zero;
		}
	}
}
