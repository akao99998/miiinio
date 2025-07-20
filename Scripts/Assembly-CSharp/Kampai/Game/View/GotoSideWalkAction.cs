using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class GotoSideWalkAction : KampaiAction
	{
		private CharacterObject minionObject;

		private Building building;

		private Vector3 lastPosition;

		private IDefinitionService definitionService;

		private Signal<CharacterObject> relocateSignal;

		private int pathIndex;

		private GoTween tween;

		public GotoSideWalkAction(CharacterObject minionObj, Building building, IKampaiLogger logger, IDefinitionService definitionService, Signal<CharacterObject> relocateSignal, int preferredPathIndex = 0)
			: base(logger)
		{
			minionObject = minionObj;
			this.building = building;
			this.definitionService = definitionService;
			this.relocateSignal = relocateSignal;
			pathIndex = preferredPathIndex;
		}

		public override void Execute()
		{
			string buildingFootprint = definitionService.GetBuildingFootprint(building.Definition.FootprintID);
			Point point;
			if (BuildingUtil.HasSidewalk(buildingFootprint))
			{
				if (pathIndex == 0)
				{
					point = BuildingUtil.GetClosestBuildingSidewalk(building.Location, minionObject.transform.position, buildingFootprint);
				}
				else
				{
					List<Point> buildingSideWalkList = BuildingUtil.GetBuildingSideWalkList(building.Location, buildingFootprint);
					point = buildingSideWalkList[Math.Min(pathIndex, buildingSideWalkList.Count - 1)];
				}
			}
			else
			{
				if (relocateSignal != null && !(building is DebrisBuilding))
				{
					relocateSignal.Dispatch(minionObject);
					base.Done = true;
					return;
				}
				point = default(Point);
				point.XZProjection = minionObject.transform.position;
			}
			Vector3 vector = new Vector3(point.x, 0f, point.y);
			Vector3 vector2 = VectorUtils.ZeroY(vector - minionObject.transform.position);
			if (vector2 != Vector3.zero)
			{
				minionObject.transform.rotation = Quaternion.LookRotation(vector2);
			}
			lastPosition = minionObject.transform.position;
			minionObject.SetAnimBool("isMoving", true);
			tween = Go.to(minionObject.transform, 1f, new GoTweenConfig().setEaseType(GoEaseType.Linear).position(vector).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
				minionObject.SetAnimBool("isMoving", false);
				base.Done = true;
			}));
		}

		public override void Abort()
		{
			if (tween != null)
			{
				tween.complete();
			}
			base.Abort();
		}

		public override void LateUpdate()
		{
			Vector3 position = minionObject.transform.position;
			float num = Vector3.Distance(lastPosition, position);
			minionObject.SetAnimFloat("speed", Mathf.Clamp(num / Time.deltaTime, 0f, 2f));
			lastPosition = position;
		}
	}
}
