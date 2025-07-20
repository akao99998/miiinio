using System.Collections;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class CurrentQuestView : KampaiView
	{
		public KampaiImage icon;

		public MinionSlotModal MinionSlot;

		private Animator animator;

		private IFancyUIService fancyUIService;

		private DummyCharacterObject characterObject;

		private IEnumerator moveCompleteCoRoutine;

		internal void Init(int characterDefId, IFancyUIService fancyUIService, DummyCharacterType characterType)
		{
			this.fancyUIService = fancyUIService;
			animator = base.gameObject.GetComponent<Animator>();
			RectTransform rectTransform = MinionSlot.transform as RectTransform;
			rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, rectTransform.anchoredPosition3D.z + -900f);
			characterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.SelectedIdle, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, characterDefId);
		}

		internal void UpdateQuest(int characterDefId, DummyCharacterType characterType)
		{
			animator.Play("Close");
			moveCompleteCoRoutine = MoveComplete(characterDefId, characterType);
			StartCoroutine(moveCompleteCoRoutine);
		}

		public IEnumerator MoveComplete(int characterDefId, DummyCharacterType characterType)
		{
			yield return new WaitForSeconds(1f);
			RemoveCoroutine();
			if (MinionSlot != null)
			{
				characterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.SelectedIdle, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, characterDefId);
				animator.Play("Open");
			}
			moveCompleteCoRoutine = null;
		}

		internal void RemoveCoroutine()
		{
			if (moveCompleteCoRoutine != null)
			{
				StopCoroutine(moveCompleteCoRoutine);
				moveCompleteCoRoutine = null;
			}
			if (characterObject != null)
			{
				characterObject.RemoveCoroutine();
				Object.Destroy(characterObject.gameObject);
			}
		}
	}
}
