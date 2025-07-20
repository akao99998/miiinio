using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RelocateCharacterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RelocateCharacterCommand") as IKampaiLogger;

		[Inject]
		public CharacterObject characterObject { get; set; }

		[Inject(GameElement.RELOCATION_POINTS)]
		public List<Point> usedRelocationPoints { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public IRoutineRunner RoutineRunner { get; set; }

		public override void Execute()
		{
			Transform transform = characterObject.gameObject.transform;
			Point point = Point.FromVector3(transform.position);
			Queue<Point> queue = new Queue<Point>();
			environment.GetClosestWalkableGridSquares(point.x, point.y, 20, queue);
			while (queue.Count > 0)
			{
				point = queue.Dequeue();
				if (!IsOccupied(point))
				{
					if (usedRelocationPoints.Count == 0)
					{
						RoutineRunner.StartCoroutine(ClearPoints(usedRelocationPoints));
					}
					usedRelocationPoints.Add(point);
					transform.position = point.XZProjection;
					logger.Debug("Relocating {0} to ({1}, {2}) {3}", characterObject.name, point.x, point.y, queue.Count);
					return;
				}
			}
			logger.Debug("Gave up relocating {0} to ({1}, {2})", characterObject.name, point.x, point.y);
			transform.position = point.XZProjection;
		}

		private bool IsOccupied(Point point)
		{
			for (int i = 0; i < usedRelocationPoints.Count; i++)
			{
				if (usedRelocationPoints[i] == point)
				{
					return true;
				}
			}
			Collider[] array = Physics.OverlapSphere(new Vector3(point.x, 1f, point.y), 1f);
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider.gameObject.GetComponentInParent<CharacterObject>() != null)
				{
					return true;
				}
			}
			return false;
		}

		private IEnumerator ClearPoints(List<Point> points)
		{
			yield return new WaitForEndOfFrame();
			points.Clear();
		}
	}
}
