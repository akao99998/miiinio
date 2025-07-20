using System;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CameraUtils : KampaiView
	{
		private const float NearClipPlane = 0.3f;

		private const float FarClipPlane = 60f;

		private Camera mainCamera;

		private Plane groundPlane;

		private Ray ray;

		private float hitDistance;

		private float angle;

		private float width;

		private float depth;

		private Vector3 center;

		private Vector2 xBounds;

		private Vector2 zBounds;

		private int currentFrame;

		private Vector3 mainCameraRaycast;

		protected override void Start()
		{
			mainCamera = GetComponent<Camera>();
			mainCamera.nearClipPlane = 0.3f;
			mainCamera.farClipPlane = 60f;
			Vector3 inNormal = new Vector3(0f, 1f, 0f);
			Vector3 inPoint = new Vector3(0f, 0f, 0f);
			groundPlane = new Plane(inNormal, inPoint);
			xBounds = new Vector2(10f, 100f);
			zBounds = new Vector2(-20f, 70f);
			angle = base.transform.eulerAngles.y * ((float)Math.PI / 180f);
			center = new Vector3((xBounds.y + xBounds.x) / 2f, 0f, (zBounds.y + zBounds.x) / 2f);
			width = (xBounds.y - xBounds.x) / 2f;
			depth = (zBounds.y - zBounds.x) / 2f;
			base.Start();
		}

		public bool Contains(Vector3 point)
		{
			Vector2 vector = new Vector2(point.x - center.x, point.z - center.z);
			float num = Mathf.Cos(angle) * vector.x - Mathf.Sin(angle) * vector.y;
			float num2 = Mathf.Sin(angle) * vector.x + Mathf.Cos(angle) * vector.y;
			num += center.x;
			num2 += center.z;
			return num > center.x - width && num < center.x + width && num2 > center.z - depth && num2 < center.z + depth;
		}

		public Vector3 CameraCenterRaycast()
		{
			int frameCount = Time.frameCount;
			if (currentFrame != frameCount)
			{
				mainCameraRaycast = CameraCenterRaycast(mainCamera);
				currentFrame = frameCount;
			}
			return mainCameraRaycast;
		}

		public Vector3 CameraCenterRaycast(Camera camera)
		{
			return GroundPlaneRaycast(new Vector3((float)camera.pixelWidth * 0.5f, (float)camera.pixelHeight * 0.5f, 0f));
		}

		public Vector3 GroundPlaneRaycast(float xPercentage, float yPercentage)
		{
			return GroundPlaneRaycast(new Vector3((float)mainCamera.pixelWidth * xPercentage, (float)mainCamera.pixelHeight * yPercentage, 0f));
		}

		public Vector3 GroundPlaneRaycast(Vector3 screenPosition)
		{
			return GroundPlaneRaycast(screenPosition, mainCamera);
		}

		public Vector3 GroundPlaneRaycast(Vector3 screenPosition, Camera camera)
		{
			ray = camera.ScreenPointToRay(screenPosition);
			groundPlane.Raycast(ray, out hitDistance);
			return ray.GetPoint(hitDistance);
		}

		public Vector3 GroundPlaneRaycastFromPoint(Vector3 pointInSpace)
		{
			ray = new Ray(pointInSpace, mainCamera.transform.forward);
			groundPlane.Raycast(ray, out hitDistance);
			return ray.GetPoint(hitDistance);
		}

		public Vector3 UIToWorldCoords(Vector3 uiPosition)
		{
			Vector3 vector = GroundPlaneRaycast(uiPosition);
			return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		}
	}
}
