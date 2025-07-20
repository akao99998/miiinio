using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util
{
	public class PartyFavorAnimationService : IPartyFavorAnimationService
	{
		private readonly Dictionary<int, PartyFavorAnimationView> partyFavorCache = new Dictionary<int, PartyFavorAnimationView>();

		private HashSet<int> allPartyFavorItems;

		private List<int> availablePartyFavorItems = new List<int>();

		private bool initialized;

		private bool randomPartyFavorStarted;

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public DebugUpdateGridSignal debugUpdateGridSignal { get; set; }

		[Inject]
		public AddMinionsPartyFavorSignal addMinionsPartyFavorSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionPartyAnimationSignal minionPartyAnimationSignal { get; set; }

		[Inject]
		public MinionPartyFavorIncidentalAnimationSignal incidentalAnimationSignal { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public AddSpecificMinionPartyFavorSignal addSpecificMinionSignal { get; set; }

		[Inject]
		public PartyFavorTrackChildSignal trackChildSignal { get; set; }

		[Inject]
		public PartyFavorFreeAllMinionsSignal freeMinionsSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IncidentalPartyFavorAnimationCompletedSignal incidentalPartyFavorAnimationCompletedSignal { get; set; }

		public HashSet<int> GetAllPartyFavorItems()
		{
			if (allPartyFavorItems == null)
			{
				List<PartyFavorAnimationDefinition> all = definitionService.GetAll<PartyFavorAnimationDefinition>();
				allPartyFavorItems = new HashSet<int>();
				if (all != null)
				{
					foreach (PartyFavorAnimationDefinition item in all)
					{
						if (!allPartyFavorItems.Contains(item.ItemID))
						{
							allPartyFavorItems.Add(item.ItemID);
						}
					}
				}
			}
			return allPartyFavorItems;
		}

		private PartyFavorAnimationView CreatePartyFavor(int partyFavorId, int minionID, bool specificMinion = false)
		{
			return CreatePartyFavor(definitionService.Get<PartyFavorAnimationDefinition>(partyFavorId), minionID, specificMinion);
		}

		private PartyFavorAnimationView CreatePartyFavor(PartyFavorAnimationDefinition animationDefinition, int minionID, bool specificMinion = false)
		{
			int iD = animationDefinition.ID;
			if (partyFavorCache.ContainsKey(iD))
			{
				return null;
			}
			PartyFavorAnimationView partyFavorAnimationView = InstantiatePartyFavor(animationDefinition);
			partyFavorCache.Add(iD, partyFavorAnimationView);
			if (specificMinion)
			{
				addSpecificMinionSignal.Dispatch(iD, minionID);
			}
			else
			{
				addMinionsPartyFavorSignal.Dispatch(iD);
			}
			return partyFavorAnimationView;
		}

		private PartyFavorAnimationView InstantiatePartyFavor(PartyFavorAnimationDefinition def)
		{
			GameObject gameObject = new GameObject();
			PartyFavorAnimationView partyFavorAnimationView = gameObject.AddComponent<PartyFavorAnimationView>();
			gameObject.name = def.LocalizedKey;
			Transform transform = gameObject.transform;
			gameObject.transform.SetParent(buildingManager.transform, false);
			Vector3 localEulerAngles = (transform.localPosition = Vector3.zero);
			transform.localEulerAngles = localEulerAngles;
			localEulerAngles = (transform.eulerAngles = Vector3.one);
			transform.localScale = localEulerAngles;
			gameObject.SetLayerRecursively(9);
			partyFavorAnimationView.Init(def, definitionService, pathFinder, debugUpdateGridSignal, environment, delegate(int minionId)
			{
				if (randomPartyFavorStarted)
				{
					PlayRandomIncidentalAnimation(minionId);
				}
				else
				{
					incidentalPartyFavorAnimationCompletedSignal.Dispatch(def.ID);
				}
			});
			return partyFavorAnimationView;
		}

		private void Init()
		{
			List<PartyFavorAnimationDefinition> all = definitionService.GetAll<PartyFavorAnimationDefinition>();
			if (all == null)
			{
				return;
			}
			foreach (PartyFavorAnimationDefinition item in all)
			{
				if (playerService.GetQuantityByDefinitionId(item.UnlockId) != 0)
				{
					AddAvailablePartyFavorItem(item.ID);
				}
			}
		}

		public List<int> GetAvailablePartyFavorItems()
		{
			if (!initialized)
			{
				initialized = true;
				Init();
			}
			return availablePartyFavorItems;
		}

		public void AddAvailablePartyFavorItem(int ID)
		{
			if (!availablePartyFavorItems.Contains(ID))
			{
				availablePartyFavorItems.Add(ID);
			}
		}

		public bool PlayRandomIncidentalAnimation(int minionID)
		{
			int count = GetAvailablePartyFavorItems().Count;
			if (count <= 0)
			{
				return false;
			}
			int index = randomService.NextInt(count);
			int id = GetAvailablePartyFavorItems()[index];
			PartyFavorAnimationDefinition partyFavorAnimationDefinition = definitionService.Get<PartyFavorAnimationDefinition>(id);
			PartyFavorAnimationView partyFavorAnimationView = CreatePartyFavor(partyFavorAnimationDefinition, minionID, true);
			if (partyFavorAnimationView == null)
			{
				return false;
			}
			GetAvailablePartyFavorItems().Remove(partyFavorAnimationDefinition.ID);
			if (minionID > 0)
			{
				incidentalAnimationSignal.Dispatch(minionID, partyFavorAnimationDefinition);
			}
			return true;
		}

		public void CreateRandomPartyFavor(int minionId = -1)
		{
			randomPartyFavorStarted = true;
			foreach (int availablePartyFavorItem in GetAvailablePartyFavorItems())
			{
				CreatePartyFavor(availablePartyFavorItem, -1);
			}
			if (minionId > 0)
			{
				minionPartyAnimationSignal.Dispatch(minionId);
			}
		}

		public void RemoveAllPartyFavorAnimations()
		{
			List<int> list = new List<int>();
			foreach (int key in partyFavorCache.Keys)
			{
				list.Add(key);
			}
			foreach (int item in list)
			{
				ReleasePartyFavor(item);
			}
			partyFavorCache.Clear();
			randomPartyFavorStarted = false;
		}

		public void ReleasePartyFavor(int partyFavorId)
		{
			PartyFavorAnimationView partyFavorView = GetPartyFavorView(partyFavorId);
			GetAvailablePartyFavorItems().Add(partyFavorId);
			ReleasePartyFavor(partyFavorView);
		}

		private PartyFavorAnimationView GetPartyFavorView(int partyFavorId)
		{
			return (!partyFavorCache.ContainsKey(partyFavorId)) ? null : partyFavorCache[partyFavorId];
		}

		public void ReleasePartyFavor(PartyFavorAnimationView partyFavor)
		{
			if (partyFavor != null)
			{
				int iD = partyFavor.PartyFavorDefinition.ID;
				freeMinionsSignal.Dispatch(iD);
				if (partyFavor.gameObject != null)
				{
					Object.Destroy(partyFavor.gameObject);
				}
				if (partyFavorCache.ContainsKey(iD))
				{
					partyFavorCache.Remove(iD);
				}
			}
		}

		public void AddMinionsToPartyFavor(int partyFavorId, MinionObject minion)
		{
			PartyFavorAnimationView partyFavorView = GetPartyFavorView(partyFavorId);
			if (minion != null && partyFavorView != null)
			{
				routineRunner.StartCoroutine(WaitAFrame(partyFavorView.PartyFavorDefinition.ID, minion));
			}
		}

		private IEnumerator WaitAFrame(int partyFavorID, MinionObject minion)
		{
			yield return new WaitForEndOfFrame();
			trackChildSignal.Dispatch(partyFavorID, minion);
		}
	}
}
