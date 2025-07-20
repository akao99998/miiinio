using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PathAction : KampaiAction
	{
		protected ActionableObject obj;

		protected IList<Vector3> path;

		private float time = 1f;

		private float startTime = -1f;

		private GoTween tween;

		private Vector3 lastPosition;

		public Vector3 GoalPosition
		{
			get
			{
				return path[path.Count - 1];
			}
		}

		public PathAction(ActionableObject obj, IList<Vector3> path, float time, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.path = new List<Vector3>(path);
			this.time = time;
		}

		public override void Abort()
		{
			if (tween != null)
			{
				tween.destroy();
			}
			base.Done = true;
			obj.SetAnimBool("isMoving", false);
		}

		public virtual float Duration()
		{
			return time;
		}

		public virtual float RemainingTime()
		{
			return Mathf.Max(time - (Time.realtimeSinceStartup - startTime), 0f);
		}

		public override void Execute()
		{
			startTime = Time.realtimeSinceStartup;
			int num = path.Count;
			if (num > 1)
			{
				if (num > 2)
				{
					path.Insert(0, path[0]);
					num++;
					path.Add(path[num - 1]);
				}
				float sqrMagnitude = (path[0] - path[num - 1]).sqrMagnitude;
				if (sqrMagnitude < 0.0001f)
				{
					base.Done = true;
					return;
				}
				GoSpline goSpline = new GoSpline(path as List<Vector3>);
				lastPosition = obj.transform.position;
				obj.SetAnimBool("isMoving", true);
				tween = Go.to(obj.transform, Duration(), new GoTweenConfig().setEaseType(GoEaseType.Linear).positionPath(goSpline, false, GoLookAtType.NextPathNode).onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
					obj.SetAnimBool("isMoving", false);
					obj.StopLocalAudio();
					PathFinished();
				}));
			}
			else
			{
				if (num == 1)
				{
					obj.gameObject.transform.position = path[0];
				}
				PathFinished();
			}
		}

		protected virtual void PathFinished()
		{
			base.Done = true;
		}

		public override void LateUpdate()
		{
			Vector3 position = obj.transform.position;
			float num = Vector3.Distance(lastPosition, position);
			float a = num / Time.deltaTime;
			obj.SetAnimBool("isMoving", true);
			obj.SetAnimFloat("speed", Mathf.Min(a, 4.5f));
			lastPosition = position;
		}

		public override string ToString()
		{
			return string.Format("{0} : {1}", base.ToString(), VectorUtils.PathToString(path));
		}
	}
}
