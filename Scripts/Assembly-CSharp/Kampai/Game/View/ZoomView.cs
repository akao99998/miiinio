using System;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class ZoomView : KampaiView, IDefinitionsHotSwapHandler, CameraView
	{
		protected Vector3 velocity;

		protected float decayAmount;

		protected Ray mouseRay;

		protected Plane groundPlane;

		protected float hitDistance;

		protected Vector3 hitPosition;

		internal float fraction;

		protected float totalPixels;

		protected float zoomDistance;

		private Camera mainCamera;

		private float totalInches;

		private float fractionMax = 1f;

		private float fractionMin;

		private Func<float, float, float, float, float> positionEaseFunction;

		private Func<float, float, float, float, float> rotationEaseFunction;

		private Func<float, float, float, float, float> fovEaseFunction;

		internal Signal<float> zoomSignal = new Signal<float>();

		private float initialFraction = 0.4f;

		private float inverseMinTilt;

		private CameraDefinition cameraDefinition;

		private float zoomOutOffset;

		private float zoomInOffset;

		private float multiplier;

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

		public float InitialFraction
		{
			get
			{
				return initialFraction;
			}
			set
			{
				initialFraction = value;
			}
		}

		protected override void Start()
		{
			mainCamera = base.gameObject.GetComponent<Camera>();
			positionEaseFunction = GoTweenUtils.easeFunctionForType(GoEaseType.Linear);
			rotationEaseFunction = GoTweenUtils.easeFunctionForType(GoEaseType.Linear);
			fovEaseFunction = GoTweenUtils.easeFunctionForType(GoEaseType.Linear);
			float arg = initialFraction * fractionMax;
			mainCamera.transform.eulerAngles = new Vector3(rotationEaseFunction(arg, 55f, -30f, 1f), mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
			mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, positionEaseFunction(arg, 30f, -17f, 1f), mainCamera.transform.position.z);
			mainCamera.fieldOfView = fovEaseFunction(arg, 40f, -31f, 1f);
			fraction = arg;
			decayAmount = 0.75f;
			Vector3 inNormal = new Vector3(0f, 1f, 0f);
			Vector3 inPoint = new Vector3(0f, 0f, 0f);
			groundPlane = new Plane(inNormal, inPoint);
			zoomDistance = 17f;
			if (Screen.dpi == 0f)
			{
				totalPixels = 1000f;
			}
			else
			{
				if (DeviceCapabilities.IsTablet())
				{
					totalInches = 2.5f;
				}
				else
				{
					totalInches = 1.5f;
				}
				totalPixels = Screen.dpi * (totalInches / fractionMax);
			}
			inverseMinTilt = 25f / fractionMax;
			base.Start();
		}

		public void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			Init(definitionService);
		}

		public void Init(IDefinitionService definitionService)
		{
			cameraDefinition = definitionService.Get<CameraDefinition>(1000008101);
			zoomOutOffset = (fractionMin - cameraDefinition.MaxZoomOutLevel) * (1f / cameraDefinition.ZoomOutBounceSpeed);
			zoomInOffset = (cameraDefinition.MaxZoomInLevel - fractionMax) * (1f / cameraDefinition.ZoomInBounceSpeed);
		}

		public virtual void CalculateBehaviour(Vector3 position)
		{
		}

		public virtual void PerformBehaviour(CameraUtils cameraUtils)
		{
			if (IsInputStationary())
			{
				return;
			}
			if (IsInputDone())
			{
				if (fraction > fractionMax)
				{
					fraction -= zoomInOffset * Time.deltaTime;
					velocity = Vector3.zero;
				}
				else if (fraction < fractionMin)
				{
					fraction += zoomOutOffset * Time.deltaTime;
					velocity = Vector3.zero;
				}
			}
			float num = velocity.y / totalPixels;
			fraction += num;
			fraction = Mathf.Clamp(fraction, cameraDefinition.MaxZoomOutLevel, cameraDefinition.MaxZoomInLevel);
			base.transform.eulerAngles = new Vector3(rotationEaseFunction(fraction, 55f, -30f, 1f), base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			base.transform.position = new Vector3(base.transform.position.x, positionEaseFunction(fraction, 30f, -17f, 1f), base.transform.position.z);
			mainCamera.fieldOfView = fovEaseFunction(fraction, 40f, -31f, 1f);
			zoomSignal.Dispatch(fraction);
		}

		protected virtual bool IsInputStationary()
		{
			return false;
		}

		protected virtual bool IsInputDone()
		{
			return false;
		}

		public virtual void ResetBehaviour()
		{
		}

		public virtual void Decay()
		{
			velocity *= decayAmount;
		}

		public virtual void SetupAutoZoom(float zoomTo)
		{
			multiplier = zoomTo - fraction;
		}

		public virtual void PerformAutoZoom(float delta)
		{
			float num = delta * multiplier;
			fraction += num;
			base.transform.eulerAngles = new Vector3(rotationEaseFunction(fraction, 55f, -30f, 1f), base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			base.transform.position = new Vector3(base.transform.position.x, positionEaseFunction(fraction, 30f, -17f, 1f), base.transform.position.z);
			mainCamera.fieldOfView = fovEaseFunction(fraction, 40f, -31f, 1f);
			zoomSignal.Dispatch(fraction);
		}

		internal float GetCurrentPercentage()
		{
			return 1f - (base.transform.eulerAngles.x - inverseMinTilt) / (55f - inverseMinTilt);
		}

		internal void UpdateFraction()
		{
			fraction = GetCurrentPercentage();
		}
	}
}
