using System.Collections.Generic;
using System.Linq;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class PartyService : IPartyService
	{
		private LevelFunTable levelFunList;

		private bool m_isInspired;

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		public bool IsInspiredParty
		{
			get
			{
				return m_isInspired;
			}
			set
			{
				m_isInspired = value;
			}
		}

		private int ClampLevel(int level)
		{
			if (levelFunList == null)
			{
				levelFunList = DefinitionService.Get<LevelFunTable>(1000009681);
			}
			level = Mathf.Clamp(level, 0, levelFunList.partiesNeededList.Count - 1);
			return level;
		}

		private int ClampPartyIndex(List<int> pointsNeededList, int partyIndex)
		{
			partyIndex = Mathf.Clamp(partyIndex, 0, pointsNeededList.Count - 1);
			return partyIndex;
		}

		public int GetTotalParties(int level)
		{
			if (levelFunList == null)
			{
				levelFunList = DefinitionService.Get<LevelFunTable>(1000009681);
			}
			level = ClampLevel(level);
			return levelFunList.partiesNeededList[level].PointsNeeded.Count;
		}

		public uint GetTotalPartyPoints(int level, int partyIndex)
		{
			if (levelFunList == null)
			{
				levelFunList = DefinitionService.Get<LevelFunTable>(1000009681);
			}
			level = ClampLevel(level);
			List<int> pointsNeeded = levelFunList.partiesNeededList[level].PointsNeeded;
			return (uint)pointsNeeded[ClampPartyIndex(pointsNeeded, partyIndex)];
		}

		public uint GetTotalPartyPoints(int level, int fromPartyIndex, int toPartyIndex)
		{
			uint num = 0u;
			for (int i = fromPartyIndex; i <= toPartyIndex; i++)
			{
				num += GetTotalPartyPoints(level, i);
			}
			return num;
		}

		public bool IsInspirationParty(int level, int currentIndex)
		{
			IsInspiredParty = currentIndex >= levelFunList.partiesNeededList[level].PointsNeeded.Count - 1;
			return IsInspiredParty;
		}

		public void GetNewLevelIndexAndPointsAfterParty(int level, int currentIndex, int currentPoints, out int newLevel, out int newIndex, out int newPoints)
		{
			newPoints = currentPoints - (int)GetTotalPartyPoints(level, currentIndex);
			if (IsInspirationParty(level, currentIndex))
			{
				newLevel = level + 1;
				newIndex = 0;
			}
			else
			{
				newLevel = level;
				newIndex = currentIndex + 1;
			}
		}

		public int GetCumulativePointsEarnedThisLevel(int level, int currentIndex, int currentPartyPoints)
		{
			int num = levelFunList.partiesNeededList[level].PointsNeeded.GetRange(0, currentIndex).Sum();
			return num + currentPartyPoints;
		}

		public int GetCumulativePointsRequiredThisLevel(int currentLevel)
		{
			if (levelFunList == null)
			{
				levelFunList = DefinitionService.Get<LevelFunTable>(1000009681);
			}
			return levelFunList.partiesNeededList[currentLevel].PointsNeeded.Sum();
		}

		public int GetCumulativePointsNeededForNextParty(int level, int currentIndex)
		{
			return (int)GetTotalPartyPoints(level, 0, currentIndex);
		}

		public Tuple<int, int> V4toV5UpdatePartyPointsAndIndex(int level, int xp)
		{
			if (levelFunList == null)
			{
				levelFunList = DefinitionService.Get<LevelFunTable>(1000009681);
			}
			List<int> pointsNeeded = levelFunList.partiesNeededList[level].PointsNeeded;
			int num = 0;
			level = ClampLevel(level);
			int cumulativePointsRequiredThisLevel = GetCumulativePointsRequiredThisLevel(level);
			if (xp >= cumulativePointsRequiredThisLevel)
			{
				num = pointsNeeded.Count - 1;
				return new Tuple<int, int>(pointsNeeded[num], num);
			}
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int first = xp;
			int second = 0;
			int totalParties = GetTotalParties(level);
			for (int i = 0; i < totalParties; i++)
			{
				int num6 = pointsNeeded[i];
				num4 += num6;
				if (xp < num4)
				{
					second = num2;
					first = xp - (num5 - num3);
					break;
				}
				num2 = i;
				num3 = num6;
				num5 = num4;
			}
			return new Tuple<int, int>(first, second);
		}
	}
}
