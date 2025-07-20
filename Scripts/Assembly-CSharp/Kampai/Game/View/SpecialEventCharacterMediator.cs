using System.Collections;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class SpecialEventCharacterMediator : FrolicCharacterMediator
	{
		private bool isInParty;

		private IEnumerator m_showWithDelay;

		[Inject]
		public SpecialEventCharacterView characterView { get; set; }

		[Inject]
		public HideSpecialEventCharacterSignal hideSpecialEventCharacterSignal { get; set; }

		[Inject]
		public ShowSpecialEventCharacterSignal showSpecialEventCharacterSignal { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			hideSpecialEventCharacterSignal.AddListener(HideSECCharacter);
			characterView.RemoveCharacterSignal.AddListener(RemoveSECCharacter);
			characterView.NextPartyAnimSignal.AddListener(PlayPartyAnimation);
			showSpecialEventCharacterSignal.AddListener(ShowSpecialEventCharacter);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			hideSpecialEventCharacterSignal.RemoveListener(HideSECCharacter);
			characterView.RemoveCharacterSignal.RemoveListener(RemoveSECCharacter);
			characterView.NextPartyAnimSignal.RemoveListener(PlayPartyAnimation);
			showSpecialEventCharacterSignal.RemoveListener(ShowSpecialEventCharacter);
		}

		private void ShowSpecialEventCharacter(float delayLength, int specialEventCharacterID)
		{
			if (specialEventCharacterID == -1 || characterView.eventCharacter.ID == specialEventCharacterID)
			{
				m_showWithDelay = ShowWithDelay(delayLength);
				StartCoroutine(m_showWithDelay);
			}
		}

		private void HideSECCharacter(bool completedQuest)
		{
			characterView.HideSpecialEventCharacter(completedQuest);
		}

		private void RemoveSECCharacter()
		{
			Object.Destroy(base.gameObject);
			NamedCharacterManagerView.Remove(309);
		}

		protected override void StartParty()
		{
			isInParty = true;
			PlayPartyAnimation();
		}

		protected override void EndParty()
		{
			isInParty = false;
			base.frolicSignal.Dispatch(characterView.ID);
		}

		private void PlayPartyAnimation()
		{
			if (isInParty)
			{
				int partyAnimationId = base.definitionService.Get<SpecialEventCharacterDefinition>(characterView.DefinitionID).PartyAnimationId;
				WeightedInstance weightedInstance = base.playerService.GetWeightedInstance(partyAnimationId);
				MinionAnimationDefinition minionAnimationDefinition = base.definitionService.Get<MinionAnimationDefinition>(weightedInstance.NextPick(randomService).ID);
				if (minionAnimationDefinition != null)
				{
					characterView.PlayPartyAnimation(minionAnimationDefinition);
				}
			}
			else
			{
				RuntimeAnimatorController animController = KampaiResources.Load<RuntimeAnimatorController>("asm_unique_sales_minion_intro");
				characterView.SetAnimController(animController);
			}
		}

		internal IEnumerator ShowWithDelay(float delayLength)
		{
			yield return new WaitForSeconds(delayLength);
			characterView.ShowSpecialEventCharacter();
		}
	}
}
