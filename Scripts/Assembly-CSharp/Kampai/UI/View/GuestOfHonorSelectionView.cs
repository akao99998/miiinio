using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class GuestOfHonorSelectionView : PopupMenuView
	{
		public RectTransform characterPanel;

		public float padding = 10f;

		private GOH_InfoPanelView[] characterViewArray;

		public Text titleDescription;

		public Button startButton;

		private GameObject GOHCard_prefab;

		private float GOHCard_width;

		private List<int> characterList;

		internal int characterListCount;

		internal int currentCharacterIndex;

		internal int unlockedCharacterCount;

		private IPrestigeService prestigeService;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private IGuestOfHonorService guestOfHonorService;

		private ScrollRect characterPanelScroll;

		private float desiredPanelPosition = 0.5f;

		private bool characterScrollLocked = true;

		public Signal rushGOHCooldown_Callback = new Signal();

		public void Init(IPrestigeService prestigeService, IDefinitionService definitionService, IPlayerService playerService, IGuestOfHonorService guestOfHonorService)
		{
			base.Init();
			this.prestigeService = prestigeService;
			this.definitionService = definitionService;
			this.playerService = playerService;
			this.guestOfHonorService = guestOfHonorService;
			resetInternalVariables();
			SetupCharacterList(this.guestOfHonorService.GetAllGOHStates());
			if (unlockedCharacterCount == 1)
			{
				List<int> list = new List<int>();
				list.Add(characterList[0]);
				characterList = list;
				base.gameObject.GetComponentInChildren<ScrollRect>().enabled = false;
			}
			GOHCard_prefab = KampaiResources.Load("cmp_GuestOfHonor_Info") as GameObject;
			GOHCard_width = (GOHCard_prefab.transform as RectTransform).sizeDelta.x;
			characterPanelScroll = base.gameObject.GetComponentInChildren<ScrollRect>();
			characterListCount = characterList.Count;
			PopulateGOHScrollView(0);
			base.Open();
		}

		public override void FinishedOpening()
		{
			characterScrollLocked = false;
		}

		public void Update()
		{
			if (characterScrollLocked)
			{
				SetHorizontalScrollPosition(desiredPanelPosition);
			}
		}

		internal int GetCharacterPrestigeDefID(int index)
		{
			return characterList[index];
		}

		private void SetupCharacterList(Dictionary<int, bool> characters)
		{
			List<Prestige> list = new List<Prestige>();
			List<PrestigeDefinition> list2 = new List<PrestigeDefinition>();
			foreach (KeyValuePair<int, bool> character in characters)
			{
				if (character.Value)
				{
					list.Add(playerService.GetFirstInstanceByDefinitionId<Prestige>(character.Key));
				}
				else
				{
					list2.Add(definitionService.Get<PrestigeDefinition>(character.Key));
				}
			}
			list.Sort((Prestige x, Prestige y) => y.UTCTimeUnlocked.CompareTo(x.UTCTimeUnlocked));
			list2.Sort((PrestigeDefinition x, PrestigeDefinition y) => x.PreUnlockLevel.CompareTo(y.PreUnlockLevel));
			foreach (Prestige item in list)
			{
				characterList.Add(item.Definition.ID);
				unlockedCharacterCount++;
			}
			foreach (PrestigeDefinition item2 in list2)
			{
				characterList.Add(item2.ID);
			}
		}

		internal void PopulateGOHScrollView(int initiallySelectedIndex)
		{
			characterViewArray = new GOH_InfoPanelView[characterListCount];
			for (int i = 0; i < characterListCount; i++)
			{
				GameObject gameObject = Object.Instantiate(GOHCard_prefab);
				GOH_InfoPanelView component = gameObject.GetComponent<GOH_InfoPanelView>();
				characterViewArray[i] = component;
				RectTransform rectTransform = gameObject.transform as RectTransform;
				gameObject.transform.SetParent(characterPanel, false);
				int characterPrestigeDefID = GetCharacterPrestigeDefID(i);
				bool flag = i < unlockedCharacterCount;
				int currentCooldownCount = GetCurrentCooldownCount(characterPrestigeDefID);
				component.myIndex = i;
				component.prestigeDefID = characterList[i];
				component.isLocked = !flag;
				component.cooldown = currentCooldownCount;
				component.rushCallBack = rushGOHCooldown_Callback;
				component.initiallySelected = i == initiallySelectedIndex;
				rectTransform.offsetMin = new Vector2(GOHCard_width * (float)i + padding * (float)i, 0f);
				rectTransform.offsetMax = new Vector2(GOHCard_width * (float)(i + 1) + padding * (float)i, 0f);
			}
			characterPanel.sizeDelta = new Vector2(GOHCard_width * (float)characterListCount + padding * (float)characterListCount, 0f);
			characterPanel.localPosition = new Vector2(0f, characterPanel.localPosition.y);
			if (characterListCount > 1)
			{
				desiredPanelPosition = 0f;
			}
		}

		internal float GetHorizontalScrollPosition()
		{
			return characterPanelScroll.horizontalNormalizedPosition;
		}

		internal void SetHorizontalScrollPosition(float position)
		{
			if (characterPanelScroll != null)
			{
				characterPanelScroll.horizontalNormalizedPosition = position;
			}
		}

		internal int GetCurrentCooldownCount(int prestigeDefinitionID)
		{
			Prestige prestige = prestigeService.GetPrestige(prestigeDefinitionID, false);
			if (prestige != null)
			{
				return guestOfHonorService.GetPartyCooldownForPrestige(prestige.ID);
			}
			return -1;
		}

		internal void SetStartButtonUnlocked(bool unlocked)
		{
			startButton.interactable = unlocked;
		}

		internal void RushCurrentCharacterCooldown()
		{
			SetStartButtonUnlocked(true);
			characterViewArray[currentCharacterIndex].RushCharacterCooldown();
		}

		internal void Close()
		{
			removeCharacterView();
			base.Close();
		}

		internal void Hide()
		{
			removeCharacterView();
		}

		private void resetInternalVariables()
		{
			characterViewArray = new GOH_InfoPanelView[1];
			characterList = new List<int>();
			characterListCount = 0;
			currentCharacterIndex = -1;
			unlockedCharacterCount = 0;
		}

		private void removeCharacterView()
		{
			GOH_InfoPanelView[] array = characterViewArray;
			foreach (GOH_InfoPanelView gOH_InfoPanelView in array)
			{
				gOH_InfoPanelView.DestroyDummyObject();
				Object.Destroy(gOH_InfoPanelView);
			}
		}
	}
}
