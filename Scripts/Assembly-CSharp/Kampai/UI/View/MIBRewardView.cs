using System.Collections;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MIBRewardView : strange.extensions.mediation.impl.View
	{
		private const string animationStateName = "anim_RewardSpinner";

		public KampaiImage image;

		public Animator animator;

		private ItemDefinition[] itemDefs;

		private ItemDefinition pickedItemDef;

		private SpinnerAudio spinnerAudio;

		internal Signal<TransactionDefinition, Vector3> mibRewardAnimationCompleteSignal = new Signal<TransactionDefinition, Vector3>();

		internal void Init(TransactionDefinition pickedTransactionDef, ItemDefinition pickedItemDef, ItemDefinition[] itemDefs, Signal<string> playGlobalSFXSignal)
		{
			this.pickedItemDef = pickedItemDef;
			this.itemDefs = itemDefs;
			animator.Play("anim_RewardSpinner");
			spinnerAudio = base.gameObject.AddComponent<SpinnerAudio>();
			spinnerAudio.PlaySFXSignal = playGlobalSFXSignal;
			StartCoroutine(WaitTillAnimationCompletes(pickedTransactionDef));
		}

		private void SetImage(ItemDefinition itemDef)
		{
			image.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
			image.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
		}

		private IEnumerator WaitTillAnimationCompletes(TransactionDefinition pickedTransactionDef)
		{
			int spriteAnimationIndex = 0;
			spinnerAudio.StartSpinningSound();
			while (IsAnimationPlaying())
			{
				SetImage(itemDefs[spriteAnimationIndex]);
				int num = spriteAnimationIndex + 1;
				spriteAnimationIndex = num % itemDefs.Length;
				yield return new WaitForSeconds(0.2f);
			}
			spinnerAudio.StopSpinningSound();
			SetImage(pickedItemDef);
			yield return new WaitForSeconds(1f);
			mibRewardAnimationCompleteSignal.Dispatch(pickedTransactionDef, image.transform.position);
		}

		private bool IsAnimationPlaying()
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.IsName("anim_RewardSpinner") && currentAnimatorStateInfo.normalizedTime < 1f;
		}
	}
}
