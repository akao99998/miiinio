using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.UI
{
	public class PositionService : IPositionService
	{
		private List<HudElementToAvoid> hudElementsToAvoid = new List<HudElementToAvoid>();

		private Vector3[] viewportBoundaryCorners = new Vector3[4];

		[Inject(MainElement.CAMERA)]
		public Camera MainCamera { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera UICamera { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public void AddHUDElementToAvoid(GameObject toAppend, bool isCircleShape = false)
		{
			HudElementToAvoid item = new HudElementToAvoid(toAppend, isCircleShape);
			if (!hudElementsToAvoid.Contains(item))
			{
				hudElementsToAvoid.Add(item);
			}
		}

		public void RemoveHUDElementToAvoid(GameObject toRemove)
		{
			IList<HudElementToAvoid> list = new List<HudElementToAvoid>();
			foreach (HudElementToAvoid item in hudElementsToAvoid)
			{
				if (item.GameObject == null || item.Contains(toRemove))
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					hudElementsToAvoid.Remove(list[i]);
				}
			}
		}

		public PositionData GetPositionData(Vector3 worldPosition)
		{
			Vector3 vector = MainCamera.WorldToViewportPoint(worldPosition);
			Vector3 worldPositionInUI = UICamera.ViewportToWorldPoint(vector);
			return new PositionData(worldPositionInUI, vector);
		}

		public Vector2 GetUIAnchorRatioPosition(Vector3 worldPosition)
		{
			Vector3 vector = MainCamera.WorldToViewportPoint(worldPosition);
			return new Vector2(vector.x, vector.y);
		}

		public Vector2 GetUIAnchorRatioPosition(int buildingInstanceID)
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(buildingInstanceID);
			return GetUIAnchorRatioPosition(buildingObject.Center);
		}

		public SnappablePositionData GetSnappablePositionData(PositionData normalPositionData, ViewportBoundary boundary, bool avoidHudElements = false)
		{
			Vector3 viewportPosition = normalPositionData.ViewportPosition;
			Vector3 vector = ClampViewportPosition(viewportPosition, boundary, avoidHudElements);
			Vector3 clampedWorldPositionInUI = UICamera.ViewportToWorldPoint(vector);
			return new SnappablePositionData(normalPositionData.WorldPositionInUI, clampedWorldPositionInUI, viewportPosition, vector);
		}

		private ViewportBoundary GetViewportBoundary(RectTransform transform)
		{
			transform.GetWorldCorners(viewportBoundaryCorners);
			Vector2 vector = UICamera.WorldToViewportPoint(viewportBoundaryCorners[0]);
			Vector2 vector2 = UICamera.WorldToViewportPoint(viewportBoundaryCorners[2]);
			Vector2 vector3 = new Vector2(vector.x + (vector2.x - vector.x) / 2f, vector.y + (vector2.y - vector.y) / 2f);
			if (vector3.x <= 0.35f && vector3.y <= 0.35f)
			{
				return new ViewportBoundary(0f, 0f, vector2.y, vector2.x);
			}
			if (vector3.x <= 0.35f && vector3.y > 0.65f)
			{
				return new ViewportBoundary(vector.y, 0f, 1f, vector2.x);
			}
			if (vector3.x > 0.65f && vector3.y > 0.65f)
			{
				return new ViewportBoundary(vector.y, vector.x, 1f, 1f);
			}
			if (vector3.x > 0.65f && vector3.y <= 0.35f)
			{
				return new ViewportBoundary(0f, vector.x, vector2.y, 1f);
			}
			if (vector3.y > 0.65f)
			{
				return new ViewportBoundary(vector.y, vector.x, 1f, vector2.x);
			}
			if (vector3.y <= 0.35f)
			{
				return new ViewportBoundary(0f, vector.x, vector2.y, vector2.x);
			}
			if (vector3.x <= 0.35f)
			{
				return new ViewportBoundary(vector.y, 0f, vector2.y, vector2.x);
			}
			return new ViewportBoundary(vector.y, vector.x, vector2.y, 1f);
		}

		private Vector3 ClampViewportPosition(Vector3 viewportPosition, ViewportBoundary boundary, bool avoidHudElements)
		{
			float x = viewportPosition.x;
			float left = boundary.Left;
			float right = boundary.Right;
			viewportPosition.x = ((x < left) ? left : ((!(x > right)) ? x : right));
			float y = viewportPosition.y;
			float top = boundary.Top;
			float bottom = boundary.Bottom;
			viewportPosition.y = ((y < bottom) ? bottom : ((!(y > top)) ? y : top));
			if (avoidHudElements && hudElementsToAvoid != null)
			{
				List<HudElementToAvoid>.Enumerator enumerator = hudElementsToAvoid.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GameObject gameObject = enumerator.Current.GameObject;
						bool isCircleShape = enumerator.Current.IsCircleShape;
						if (!(gameObject != null) || !gameObject.activeInHierarchy)
						{
							continue;
						}
						if (!isCircleShape)
						{
							ViewportBoundary viewportBoundary = GetViewportBoundary(gameObject.transform as RectTransform);
							if (viewportBoundary.Contains(viewportPosition))
							{
								viewportPosition = ClampViewportPositionForBoundary(x, y, viewportBoundary);
							}
						}
						else
						{
							viewportPosition = ClampWithinSphere(viewportPosition, gameObject);
						}
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			return viewportPosition;
		}

		private Vector3 ClampWithinSphere(Vector3 viewportPosition, GameObject go)
		{
			RectTransform rectTransform = go.transform as RectTransform;
			if (rectTransform != null)
			{
				return ClampWithinSphere2D(viewportPosition, rectTransform);
			}
			return ClampWithinSphere3D(viewportPosition, go.GetComponent<Renderer>());
		}

		private Vector3 ClampWithinSphere2D(Vector3 viewportPosition, RectTransform rectTransform)
		{
			Vector3 result = viewportPosition;
			Vector3 vector = UICamera.ViewportToWorldPoint(viewportPosition);
			rectTransform.GetWorldCorners(viewportBoundaryCorners);
			float num = (viewportBoundaryCorners[2].y - viewportBoundaryCorners[0].y) / 2f;
			Vector3 position = rectTransform.position;
			Vector3 vector2 = vector - position;
			vector2.z = 0f;
			if (vector2.sqrMagnitude <= num * num)
			{
				Vector3 position2 = position + vector2.normalized * num;
				result = UICamera.WorldToViewportPoint(position2);
			}
			return result;
		}

		private Vector3 ClampWithinSphere3D(Vector3 viewportPosition, Renderer renderer)
		{
			Vector3 result = viewportPosition;
			Vector3 vector = UICamera.ViewportToWorldPoint(viewportPosition);
			Vector3 center = renderer.bounds.center;
			center.z = vector.z;
			float num = Mathf.Abs(renderer.bounds.max.y - renderer.bounds.min.y) / 2f;
			Vector3 vector2 = vector - center;
			if (vector2.sqrMagnitude <= num * num)
			{
				Vector3 position = center + vector2.normalized * num;
				result = UICamera.WorldToViewportPoint(position);
			}
			return result;
		}

		private Vector2 ClampViewportPositionForBoundary(float originalX, float originalY, ViewportBoundary boundary)
		{
			bool flag = Mathf.Approximately(boundary.Bottom, 0f);
			bool flag2 = Mathf.Approximately(boundary.Left, 0f);
			bool flag3 = Mathf.Approximately(boundary.Right, 1f);
			bool flag4 = Mathf.Approximately(boundary.Top, 1f);
			float x = Mathf.Clamp(originalX, boundary.Left, boundary.Right);
			float y = Mathf.Clamp(originalY, boundary.Bottom, boundary.Top);
			if (flag2)
			{
				if ((flag || flag4) && originalX <= boundary.Right)
				{
					return new Vector2(x, (!flag) ? boundary.Bottom : boundary.Top);
				}
				return new Vector2(boundary.Right, y);
			}
			if (flag3)
			{
				if ((flag || flag4) && originalX >= boundary.Left)
				{
					return new Vector2(x, (!flag) ? boundary.Bottom : boundary.Top);
				}
				return new Vector2(boundary.Left, y);
			}
			return new Vector2(x, (!flag) ? boundary.Bottom : boundary.Top);
		}
	}
}
