using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class IncidentalAnimationCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("IncidentalAnimationCommand") as IKampaiLogger;

		private MinionManagerView mm;

		private int minionCount;

		[Inject]
		public int minionID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public StartIncidentalAnimationSignal incidentalSignal { get; set; }

		[Inject]
		public MinionAcknowledgeSignal acknowledgeSignal { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public StartGroupGachaSignal startGroupGachaSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorAnimationService { get; set; }

		public override void Execute()
		{
			mm = minionManager.GetComponent<MinionManagerView>();
			if (!mm.HasObject(minionID))
			{
				BobCharacter byInstanceId = playerService.GetByInstanceId<BobCharacter>(minionID);
				if (byInstanceId != null)
				{
					frolicSignal.Dispatch(minionID);
				}
				return;
			}
			Vector3 objectPosition = mm.GetObjectPosition(minionID);
			Point point = Point.FromVector3(objectPosition);
			Point ur = point;
			bool party = definitionService.Get<MinionPartyDefinition>(80000).Contains(point);
			Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(minionID);
			if (!byInstanceId2.IsInMinionParty)
			{
				int num = 2;
				point.x -= num;
				point.y -= num;
				ur.x += num;
				ur.y += num;
				List<MinionObject> idleMinions = GetIdleMinions(point, ur);
				minionCount = idleMinions.Count;
				if (minionCount >= 2)
				{
					MinionInteraction(objectPosition, idleMinions, party);
				}
				else if (!BuildingInteraction(objectPosition, point, ur))
				{
					PlayIncidental(party);
				}
			}
		}

		private void PlayIncidental(bool party)
		{
			StaticItem defId = ((!party) ? StaticItem.WEIGHTED_INCIDENTAL_ANIM : StaticItem.WEIGHTED_PARTY_ANIM);
			QuantityItem quantityItem = playerService.GetWeightedInstance((int)defId).NextPick(randomService);
			if (quantityItem.ID == 4036)
			{
				partyFavorAnimationService.PlayRandomIncidentalAnimation(minionID);
			}
			else
			{
				incidentalSignal.Dispatch(minionID, quantityItem.ID);
			}
		}

		private List<MinionObject> GetIdleMinions(Point ll, Point ur)
		{
			List<ActionableObject> list = new List<ActionableObject>();
			mm.GetObjectsInArea(ll, ur, list);
			List<MinionObject> list2 = new List<MinionObject>(list.Count);
			foreach (MinionObject item in list)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(item.ID);
				if (byInstanceId.State != MinionState.Idle || byInstanceId.IsInIncidental)
				{
					continue;
				}
				if (byInstanceId.HasPrestige)
				{
					Prestige byInstanceId2 = playerService.GetByInstanceId<Prestige>(byInstanceId.PrestigeId);
					if (byInstanceId2 != null && byInstanceId2.state == PrestigeState.Questing)
					{
						continue;
					}
				}
				byInstanceId.IsInIncidental = true;
				list2.Add(item);
			}
			return list2;
		}

		private void MinionInteraction(Vector3 pos, IEnumerable<MinionObject> idleMinions, bool party)
		{
			StaticItem defId = ((!party) ? StaticItem.WEIGHTED_ACK_ANIM : StaticItem.WEIGHTED_ACK_ANIM_PARTY);
			WeightedInstance weightedInstance = playerService.GetWeightedInstance((int)defId);
			QuantityItem quantityItem = weightedInstance.NextPick(randomService);
			if (quantityItem.ID == 11000)
			{
				GachaAwareness(pos, idleMinions, party);
			}
			else
			{
				MinionAwareness(pos, idleMinions, quantityItem);
			}
		}

		private void GachaAwareness(Vector3 pos, IEnumerable<MinionObject> idleMinions, bool party)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (MinionObject idleMinion in idleMinions)
			{
				hashSet.Add(idleMinion.ID);
			}
			startGroupGachaSignal.Dispatch(new MinionAnimationInstructions(hashSet, new Boxed<Vector3>(pos), party));
		}

		private void MinionAwareness(Vector3 pos, IEnumerable<MinionObject> idleMinions, QuantityItem pick)
		{
			int num = -1;
			DefinitionGroup definitionGroup = definitionService.Get<DefinitionGroup>(pick.ID);
			if (definitionGroup.Group.Count < 2)
			{
				logger.Log(KampaiLogLevel.Warning, "Minion Awareness: Invalid group {0}", pick.ID);
				return;
			}
			float angleFacingObject;
			foreach (MinionObject idleMinion in idleMinions)
			{
				if (idleMinion.ID == minionID)
				{
					continue;
				}
				num = idleMinion.ID;
				angleFacingObject = GetAngleFacingObject(mm.GetObjectPosition(num), pos);
				acknowledgeSignal.Dispatch(num, angleFacingObject, definitionGroup.Group[0]);
				break;
			}
			angleFacingObject = GetAngleFacingObject(pos, mm.GetObjectPosition(num));
			acknowledgeSignal.Dispatch(minionID, angleFacingObject, definitionGroup.Group[1]);
		}

		private bool BuildingInteraction(Vector3 pos, Point ll, Point ur)
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			QuantityItem quantityItem = playerService.GetWeightedInstance(4004).NextPick(randomService);
			bool flag = false;
			for (int i = ll.x; i <= ur.x; i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = ll.y; j <= ur.y; j++)
				{
					if (flag)
					{
						break;
					}
					Building building = null;
					if (environment.IsOccupied(i, j) && (building = environment.GetBuilding(i, j)) != null && !(building is ConnectableBuilding))
					{
						BuildingObject buildingObject = component.GetBuildingObject(building.ID);
						Vector3 position = buildingObject.transform.position;
						BoxCollider component2 = buildingObject.GetComponent<BoxCollider>();
						if (component2 != null)
						{
							position += component2.center;
						}
						float angleFacingObject = GetAngleFacingObject(pos, position);
						acknowledgeSignal.Dispatch(minionID, angleFacingObject, quantityItem.ID);
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		private float GetAngleFacingObject(Vector3 pos, Vector3 other)
		{
			Vector3 vector = other - pos;
			return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		}
	}
}
