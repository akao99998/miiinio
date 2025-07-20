using System;
using System.Collections;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class QuestView : KampaiView
	{
		public ScrollableButtonView button;

		public MinionSlotModal MinionSlot;

		[NonSerialized]
		public Quest quest;

		public float PaddingInPixels;

		private Animator animator;

		private DummyCharacterObject characterObject;

		private IFancyUIService fancyUIService;

		private IEnumerator moveCompleteCoRoutine;

		internal void Init(IFancyUIService fancyUIService)
		{
			this.fancyUIService = fancyUIService;
			animator = GetComponent<Animator>();
			DummyCharacterType characterType = fancyUIService.GetCharacterType(quest.GetActiveDefinition().SurfaceID);
			characterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, quest.GetActiveDefinition().SurfaceID, true, true, true);
		}

		internal void UpdateQuest(Action moveCompleteAction)
		{
			animator.Play("Close");
			moveCompleteCoRoutine = MoveComplete(moveCompleteAction);
			StartCoroutine(moveCompleteCoRoutine);
		}

		public IEnumerator MoveComplete(Action moveCompleteAction)
		{
			yield return new WaitForSeconds(1f);
			moveCompleteAction();
			RemoveCoroutine();
			DummyCharacterType characterType = fancyUIService.GetCharacterType(quest.GetActiveDefinition().SurfaceID);
			characterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, quest.GetActiveDefinition().SurfaceID, true, true, true);
			animator.Play("Open");
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
				UnityEngine.Object.Destroy(characterObject.gameObject);
			}
		}
	}
}
