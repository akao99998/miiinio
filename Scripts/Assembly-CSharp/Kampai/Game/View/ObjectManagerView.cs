using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class ObjectManagerView : ActionableObjectManagerView
	{
		protected IKampaiLogger logger = LogManager.GetClassLogger("ObjectManagerView") as IKampaiLogger;

		protected Dictionary<int, ActionableObject> objects = new Dictionary<int, ActionableObject>();

		protected Dictionary<string, RuntimeAnimatorController> animationControllers = new Dictionary<string, RuntimeAnimatorController>();

		public virtual void Init()
		{
			RuntimeAnimatorController value = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			animationControllers.Add("asm_minion_movement", value);
			ActionableObjectManagerView.ClearAllObjects();
		}

		public void CacheAnimations(IEnumerable<AnimationDefinition> animationDefinitions)
		{
			foreach (AnimationDefinition animationDefinition in animationDefinitions)
			{
				CacheAnimation(animationDefinition);
			}
		}

		public IEnumerator CacheAnimationsCoroutine<T>(IEnumerable<T> animationDefinitions) where T : AnimationDefinition
		{
			Stopwatch sw = Stopwatch.StartNew();
			foreach (T animDef in animationDefinitions)
			{
				CacheAnimation(animDef);
				if (sw.ElapsedMilliseconds > 1000)
				{
					sw = Stopwatch.StartNew();
					yield return null;
				}
			}
		}

		protected virtual void CacheAnimation(AnimationDefinition animDef)
		{
			string animationStateMachine = GetAnimationStateMachine(animDef);
			if (animationStateMachine.Length == 0)
			{
				logger.Log(KampaiLogLevel.Error, "Empty State Machine");
			}
			else if (!animationControllers.ContainsKey(animationStateMachine))
			{
				animationControllers.Add(animationStateMachine, KampaiResources.Load<RuntimeAnimatorController>(animationStateMachine));
			}
		}

		protected virtual string GetAnimationStateMachine(AnimationDefinition animDef)
		{
			MinionAnimationDefinition minionAnimationDefinition = animDef as MinionAnimationDefinition;
			if (minionAnimationDefinition != null)
			{
				return minionAnimationDefinition.StateMachine;
			}
			return string.Empty;
		}

		public virtual void Add(int id, ActionableObject obj)
		{
			if (objects.ContainsKey(id))
			{
				logger.Error("ObjectManagerView: Tried to add an object with an id that has already been added ({0})", id);
				return;
			}
			objects.Add(id, obj);
			if (ActionableObjectManagerView.allObjects.ContainsKey(id))
			{
				logger.Error("ObjectManagerView: Global objects dictionary already has an object with id {0}. Expect bugs.", id);
			}
			else
			{
				ActionableObjectManagerView.allObjects.Add(id, obj);
			}
		}

		public virtual void Add(ActionableObject obj)
		{
			Add(obj.ID, obj);
		}

		public virtual void Remove(int id)
		{
			objects.Remove(id);
			ActionableObjectManagerView.allObjects.Remove(id);
		}

		public void EnableRenderer(int minionID, bool enable)
		{
			ActionableObject value;
			if (objects.TryGetValue(minionID, out value))
			{
				value.EnableRenderers(enable);
			}
		}

		public bool HasObject(int objectId)
		{
			return objects.ContainsKey(objectId);
		}

		public Vector3 GetObjectPosition(int objectId)
		{
			ActionableObject value;
			if (objects.TryGetValue(objectId, out value))
			{
				return value.gameObject.transform.position;
			}
			return Vector3.zero;
		}

		public ActionableObject Get(int objectId)
		{
			ActionableObject value;
			objects.TryGetValue(objectId, out value);
			return value;
		}

		public GameObject GetGameObject(int objectId)
		{
			ActionableObject value;
			if (objects.TryGetValue(objectId, out value))
			{
				return value.gameObject;
			}
			return null;
		}

		public void GetPathingObjects(IList<Tuple<int, Vector3>> pathingObjects)
		{
			foreach (KeyValuePair<int, ActionableObject> @object in objects)
			{
				PathAction pathAction = @object.Value.currentAction as PathAction;
				if (pathAction != null)
				{
					pathingObjects.Add(new Tuple<int, Vector3>(@object.Key, pathAction.GoalPosition));
				}
			}
		}

		public void GetObjectsInArea(Point ll, Point ur, IList<ActionableObject> objectList)
		{
			foreach (ActionableObject value in objects.Values)
			{
				Vector3 position = value.gameObject.transform.position;
				if ((float)ll.x <= position.x && (float)ll.y <= position.z && (float)ur.x >= position.x && (float)ur.y >= position.z)
				{
					objectList.Add(value);
				}
			}
		}

		public virtual void StartPathing(int minionID, IList<Vector3> path, float speed, bool muteStatus, MoveMinionFinishedSignal moveFinished)
		{
			ActionableObject value;
			if (!objects.TryGetValue(minionID, out value))
			{
				logger.Error("Cannot start pathing for minion {0}, ActionableObject not in selected minions dictionary.", minionID);
			}
			else
			{
				StartPathing(value, path, speed, muteStatus, moveFinished, -180f);
			}
		}

		public virtual void StartPathing(ActionableObject mo, IList<Vector3> path, float speed, bool muteStatus, MoveMinionFinishedSignal moveFinished, float rotateOffset)
		{
			if (mo == null)
			{
				logger.Error("Cannot start pathing for minion.");
				return;
			}
			mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers["asm_minion_movement"], logger, new Dictionary<string, object> { { "IdleRandom", 0 } }), true);
			mo.EnqueueAction(new MuteAction(mo, muteStatus, logger));
			mo.EnqueueAction(new ConstantSpeedPathAction(mo, path, speed, logger));
			mo.EnqueueAction(new RotateAction(mo, Camera.main.transform.eulerAngles.y + rotateOffset, 360f, logger));
			mo.EnqueueAction(new MuteAction(mo, false, logger));
			mo.EnqueueAction(new SendIDSignalAction(mo, moveFinished, logger));
		}

		protected ICollection<ActionableObject> ToActionableObjects(ICollection<int> minionIds)
		{
			List<ActionableObject> list = new List<ActionableObject>();
			foreach (int minionId in minionIds)
			{
				list.Add(objects[minionId]);
			}
			return list;
		}
	}
}
