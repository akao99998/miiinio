using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class StickerbookModalView : PopupMenuView
	{
		public RectTransform characterPanel;

		public RectTransform stickerPanel;

		public Text stickerTitle;

		public float padding = 10f;

		private List<int> characterList = new List<int>();

		private List<StickerbookCharacterView> characterViewList = new List<StickerbookCharacterView>();

		private List<GameObject> stickerInstanceList = new List<GameObject>();

		private List<int> charactersWithUnlockedStickers;

		private GameObject stickerPackPrefab;

		private GameObject stickerPrefab;

		private float characterWidth;

		private float stickerWidth;

		private bool firstTime = true;

		internal int lastSelectedID;

		private int unlockedCharacterCount;

		public void Init(Dictionary<int, bool> characters, IDefinitionService definitionService, IPlayerService playerService)
		{
			SetupCharacterList(characters, definitionService, playerService);
			base.Init();
			CachePrefabs();
			PopulateCharacterScrollView(definitionService, playerService);
			base.Open();
		}

		internal void Close()
		{
			foreach (StickerbookCharacterView characterView in characterViewList)
			{
				characterView.RemoveCoroutine();
			}
			base.Close();
		}

		private void SetupCharacterList(Dictionary<int, bool> characters, IDefinitionService definitionService, IPlayerService playerService)
		{
			List<Prestige> list = new List<Prestige>();
			List<PrestigeDefinition> list2 = new List<PrestigeDefinition>();
			foreach (KeyValuePair<int, bool> character in characters)
			{
				if (character.Value)
				{
					Prestige firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Prestige>(character.Key);
					if (firstInstanceByDefinitionId.Definition.StickerbookDisplayableType != StickerbookCharacterDisplayableType.Never && CharacterIsDisplayable(firstInstanceByDefinitionId, definitionService, playerService))
					{
						list.Add(firstInstanceByDefinitionId);
						unlockedCharacterCount++;
					}
				}
				else
				{
					PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(character.Key);
					if (prestigeDefinition.StickerbookDisplayableType == StickerbookCharacterDisplayableType.Always)
					{
						list2.Add(prestigeDefinition);
					}
				}
			}
			list.Sort((Prestige x, Prestige y) => y.UTCTimeUnlocked.CompareTo(x.UTCTimeUnlocked));
			list2.Sort((PrestigeDefinition x, PrestigeDefinition y) => x.PreUnlockLevel.CompareTo(y.PreUnlockLevel));
			foreach (Prestige item in list)
			{
				characterList.Add(item.Definition.ID);
			}
			foreach (PrestigeDefinition item2 in list2)
			{
				characterList.Add(item2.ID);
			}
		}

		private bool CharacterIsDisplayable(Prestige prestige, IDefinitionService definitionService, IPlayerService playerService)
		{
			if (prestige.Definition.StickerbookDisplayableType == StickerbookCharacterDisplayableType.Always)
			{
				return true;
			}
			if (charactersWithUnlockedStickers == null)
			{
				List<int> list = new List<int>();
				charactersWithUnlockedStickers = new List<int>();
				foreach (PrestigeDefinition item in definitionService.GetAll<PrestigeDefinition>())
				{
					if (item.StickerbookDisplayableType == StickerbookCharacterDisplayableType.OnlyWithUnlockedStickers)
					{
						list.Add(item.ID);
					}
				}
				foreach (StickerDefinition item2 in definitionService.GetAll<StickerDefinition>())
				{
					if (list.Contains(item2.CharacterID))
					{
						Sticker firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sticker>(item2.ID);
						if (firstInstanceByDefinitionId != null)
						{
							charactersWithUnlockedStickers.Add(item2.CharacterID);
						}
					}
				}
			}
			if (charactersWithUnlockedStickers.Count == 0)
			{
				return false;
			}
			if (charactersWithUnlockedStickers.Contains(prestige.Definition.ID))
			{
				return true;
			}
			return false;
		}

		private void CachePrefabs()
		{
			stickerPackPrefab = KampaiResources.Load("cmp_StickerPackCharacters") as GameObject;
			stickerPrefab = KampaiResources.Load("cmp_Sticker") as GameObject;
			characterWidth = (stickerPackPrefab.transform as RectTransform).sizeDelta.x;
			stickerWidth = (stickerPrefab.transform as RectTransform).sizeDelta.x;
		}

		private void PopulateCharacterScrollView(IDefinitionService definitionService, IPlayerService playerService)
		{
			int num = 1;
			int count = characterList.Count;
			bool flag = false;
			bool flag2 = false;
			int num2 = 0;
			foreach (SpecialEventItemDefinition item in definitionService.GetAll<SpecialEventItemDefinition>())
			{
				SpecialEventItem firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(item.ID);
				if (item.IsActive && firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.HasEnded)
				{
					flag = true;
					break;
				}
			}
			foreach (SpecialEventItemDefinition item2 in definitionService.GetAll<SpecialEventItemDefinition>())
			{
				SpecialEventItem firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(item2.ID);
				if (firstInstanceByDefinitionId2 != null)
				{
					flag2 = true;
					break;
				}
			}
			if (flag)
			{
				num2 = 1;
				characterViewList.Add(CreateEventView(0));
			}
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(stickerPackPrefab);
				StickerbookCharacterView component = gameObject.GetComponent<StickerbookCharacterView>();
				RectTransform rectTransform = gameObject.transform as RectTransform;
				gameObject.transform.SetParent(characterPanel, false);
				component.prestigeID = characterList[i];
				component.isLimited = false;
				if (i < unlockedCharacterCount)
				{
					component.isLocked = false;
				}
				else
				{
					component.isLocked = true;
				}
				rectTransform.offsetMin = new Vector2(characterWidth * (float)(i + num2) + padding * (float)(i + num2), 0f);
				rectTransform.offsetMax = new Vector2(characterWidth * (float)(i + num2 + 1) + padding * (float)(i + num2), 0f);
				characterViewList.Add(component);
			}
			if (!flag && flag2)
			{
				characterViewList.Add(CreateEventView(count));
			}
			characterPanel.sizeDelta = new Vector2(characterWidth * (float)(count + num) + padding * (float)(count + num), 0f);
			characterPanel.localPosition = new Vector2(0f, characterPanel.localPosition.y);
		}

		private StickerbookCharacterView CreateEventView(int index)
		{
			GameObject gameObject = Object.Instantiate(stickerPackPrefab);
			StickerbookCharacterView component = gameObject.GetComponent<StickerbookCharacterView>();
			RectTransform rectTransform = gameObject.transform as RectTransform;
			gameObject.transform.SetParent(characterPanel, false);
			component.character.gameObject.SetActive(false);
			component.limitedEvent.gameObject.SetActive(true);
			component.isLimited = true;
			rectTransform.offsetMin = new Vector2(characterWidth * (float)index + padding * (float)index, 0f);
			rectTransform.offsetMax = new Vector2(characterWidth * (float)(index + 1) + padding * (float)index, 0f);
			return component;
		}

		internal void PopulateStickersForCurrentCharacter(int unlockedStickerCount, List<int> stickerList)
		{
			CleanupExistingStickers();
			for (int i = 0; i < stickerList.Count; i++)
			{
				GameObject gameObject = Object.Instantiate(stickerPrefab);
				StickerbookStickerView component = gameObject.GetComponent<StickerbookStickerView>();
				RectTransform rectTransform = gameObject.transform as RectTransform;
				gameObject.transform.SetParent(stickerPanel, false);
				if (i < unlockedStickerCount)
				{
					component.locked = false;
				}
				else
				{
					component.locked = true;
				}
				component.stickerDefinitionID = stickerList[i];
				rectTransform.offsetMin = new Vector2(stickerWidth * (float)i, 0f);
				rectTransform.offsetMax = new Vector2(stickerWidth * (float)(i + 1), 0f);
				stickerInstanceList.Add(gameObject);
			}
			stickerPanel.sizeDelta = new Vector2(stickerWidth * (float)stickerList.Count, 0f);
			stickerPanel.localPosition = new Vector2(0f, stickerPanel.localPosition.y);
		}

		private void CleanupExistingStickers()
		{
			foreach (GameObject stickerInstance in stickerInstanceList)
			{
				Object.Destroy(stickerInstance);
			}
		}

		internal void SetCharacterStrings(string characterCollection)
		{
			if (firstTime)
			{
				firstTime = false;
				stickerTitle.gameObject.SetActive(true);
			}
			stickerTitle.text = characterCollection;
		}
	}
}
